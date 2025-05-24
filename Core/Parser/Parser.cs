using Core.AST.Statements;
using Core.Expressions;
using Core.Runtime.Functions;
using Core.Runtime;
using Core.Values;
using Core.Lexer;

namespace Core.Parser;

public class Parser (List<Token> tokens)
{
    private readonly List<Token> tokens = tokens;
    private int pos;

    private readonly VariableStorage variableStorage = new();
    private readonly FunctionStorage functionStorage = new();
    private readonly ClassStorage classStorage = new();

    public List<IStatement> Parse()
    {
        List<IStatement> statements = [];

        while (Peek().Type != TokenType.EOF) statements.Add(ParseStatement());

        return statements;
    }

    private IStatement ParseStatement()
    {
        if (Match(TokenType.Class)) return ParseClassDeclaration();
        if (Match(TokenType.Let)) return ParseVariableDeclaration();
        if (Match(TokenType.Identifier))
        {
            if (Match(TokenType.LParen)) return ParseFunctionCallStatement();
            return ParseAssignment();
        }
        if (Match(TokenType.Func)) return ParseFunctionDeclaration();
        if (Match(TokenType.If)) return ParseIfElseStatement();
        if (Match(TokenType.While)) return ParseWhileLoopStatement();
        if (Match(TokenType.Do)) return ParseDoWhileLoopStatement();
        if (Match(TokenType.For)) return ParseForLoopStatement();
        if (Match(TokenType.Break)) return ParseBreakStatement();
        if (Match(TokenType.Continue)) return ParseContinueStatement();
        if (Match(TokenType.Return)) return ParseReturnStatement();

        throw new Exception($"Неожиданный токен: {Peek()}.");
    }

    private IStatement ParseClassDeclaration()
    {
        Token classNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");

        List<FieldDeclarationStatement> fields = [];
        List<MethodDeclarationStatement> methods = [];

        while (!Match(TokenType.RBrace))
        {
            AccessModifier access = AccessModifier.Private;
            if (Match(TokenType.Private)) Match(TokenType.Private);
            else if (Match(TokenType.Public)) access = AccessModifier.Public;

            if (Match(TokenType.Func)) methods.Add(ParseMethodDeclaration(access, classNameToken));
            else fields.Add(ParseFieldDeclaration(access, classNameToken));
        }

        return new ClassDeclarationStatement(classStorage, classNameToken.Value, fields, methods);
    }

    private FieldDeclarationStatement ParseFieldDeclaration(AccessModifier access, Token classNameToken)
    {
        Consume(TokenType.Let, "Отсутствует токен объявления поля 'let'.");
        Token nameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
        Token typeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления поля.");
        TypeValue type = GetTypeValueFromToken(typeToken.Type);
        IExpression? initializer = null;

        if (Match(TokenType.Assign))
            initializer = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new FieldDeclarationStatement(classStorage, classNameToken.Value, access, type, nameToken.Value, initializer);
    }

    private MethodDeclarationStatement ParseMethodDeclaration(AccessModifier access, Token classNameToken)
    {
        Token nameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        TypeValue returnType = TypeValue.Void;

        if (Match(TokenType.Colon))
        {
            Token typeToken = GetTypeToken();
            returnType = GetTypeValueFromToken(typeToken.Type);
        }

        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        List<(string, TypeValue)> parameters = [];

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");
            Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            Token paramType = GetTypeToken();
            parameters.Add((paramName.Value, GetTypeValueFromToken(paramType.Type)));
        }

        IStatement body = ParseStatementOrBlock();

        return new MethodDeclarationStatement(classStorage, classNameToken.Value, access, nameToken.Value, returnType, parameters, body);
    }

    private IStatement ParseVariableDeclaration(bool fromForStatement = false)
    {
        Token identifier = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        string identifierName = identifier.Value;
        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
        Token baseTypeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления переменной/поля.");
        TypeValue baseType = GetTypeValueFromToken(baseTypeToken.Type);
        IExpression? expression = null;
        if (Match(TokenType.Assign)) expression = ParseExpression();

        if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new VariableDeclarationStatement(variableStorage, identifierName, baseType, expression);
    }

    private IStatement ParseFunctionCallStatement()
    {
        Token identifier = Peek(-2);
        List<IExpression> args = ParseArguments();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");
        return new FunctionCallStatement(functionStorage, identifier.Value, args);
    }

    private List<IExpression> ParseArguments()
    {
        List<IExpression> args = [];
        while (!Match(TokenType.RParen))
        {
            if (args.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");
            args.Add(ParseExpression());
        }
        return args;
    }

    private IStatement ParseAssignment(bool fromForStatement = false)
    {
        string identifierName = Peek(-1).Value;
        IExpression expression;

        if (Match(TokenType.PlusAssign) || Match(TokenType.MinusAssign)
        || Match(TokenType.MultiplyAssign) || Match(TokenType.DivideAssign)
        || Match(TokenType.ModuloAssign) || Match(TokenType.Increment)
        || Match(TokenType.Decrement))
        {
            Token opToken = Peek(-1);

            if (opToken.Type == TokenType.Increment || opToken.Type == TokenType.Decrement) expression = new LiteralExpression(new IntValue(1));
            else expression = ParseExpression();

            if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            return CreateCompoundAssignment(identifierName, opToken, expression);
        }

        Consume(TokenType.Assign, "Отсутствует токен оператора присвоения.");

        expression = ParseExpression();

        if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new AssignmentStatement(variableStorage, identifierName, expression);
    }

    private IStatement CreateCompoundAssignment(string varName, Token opToken, IExpression rightExpr)
    {
        var varExpr = new VariableExpression(variableStorage, varName);

        BinaryExpression compoundExpr = opToken.Type switch
        {
            TokenType.PlusAssign => new BinaryExpression(new Token(TokenType.Plus, "+"), varExpr, rightExpr),
            TokenType.MinusAssign => new BinaryExpression(new Token(TokenType.Minus, "-"), varExpr, rightExpr),
            TokenType.MultiplyAssign => new BinaryExpression(new Token(TokenType.Multiply, "*"), varExpr, rightExpr),
            TokenType.DivideAssign => new BinaryExpression(new Token(TokenType.Divide, "/"), varExpr, rightExpr),
            TokenType.ModuloAssign => new BinaryExpression(new Token(TokenType.Modulo, "%"), varExpr, rightExpr),
            TokenType.Increment => new BinaryExpression(new Token(TokenType.Plus, "+"), varExpr, rightExpr),
            TokenType.Decrement => new BinaryExpression(new Token(TokenType.Minus, "-"), varExpr, rightExpr),
            _ => throw new Exception($"Неподдерживаемый оператор: {opToken.Value}")
        };

        return new AssignmentStatement(variableStorage, varName, compoundExpr);
    }

    private IStatement ParseFunctionDeclaration()
    {
        Token nameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        TypeValue returnType = TypeValue.Void;

        if (Match(TokenType.Colon))
        {
            Token typeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления функции/метода.");
            returnType = GetTypeValueFromToken(typeToken.Type);
        }

        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        var parameters = new List<(string, TypeValue)>();

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            Token paramType = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления параметра функции/метода.");

            parameters.Add((paramName.Value, GetTypeValueFromToken(paramType.Type)));
        }

        IStatement body = ParseStatementOrBlock();

        var function = new UserFunction(nameToken.Value, returnType, parameters, body, variableStorage);

        return new FunctionDeclarationStatement(functionStorage, nameToken.Value, function);
    }

    private IStatement ParseIfElseStatement()
    {
        Consume(TokenType.LParen, "Отсутствует токен начала условного выражения '('.");
        IExpression condition = ParseExpression();
        Consume(TokenType.RParen, "Отсутствует токен окончания условного выражения ')'.");
        IStatement ifBlock = ParseStatementOrBlock();
        IStatement? elseBlock = null;
        if (Match(TokenType.Else)) elseBlock = ParseStatementOrBlock();
        
        return new IfElseStatement(condition, ifBlock, elseBlock);
    }

    private IStatement ParseWhileLoopStatement()
    {
        Consume(TokenType.LParen, "Отсутствует токен начала условного выражения '('.");
        IExpression condition = ParseExpression();
        Consume(TokenType.RParen, "Отсутствует токен окончания условного выражения ')'.");
        IStatement block = ParseStatementOrBlock();

        return new WhileLoopStatement(variableStorage, condition, block);
    }

    private IStatement ParseDoWhileLoopStatement()
    {
        IStatement block = ParseStatementOrBlock();
        Consume(TokenType.While, "Отсутствует токен начала условной части цикла do/while 'while'.");
        Consume(TokenType.LParen, "Отсутствует токен начала условного выражения '('.");
        IExpression condition = ParseExpression();
        Consume(TokenType.RParen, "Отсутствует токен окончания условного выражения ')'.");
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new DoWhileLoopStatement(variableStorage, condition, block);
    }

    private IStatement ParseForLoopStatement()
    {
        Consume(TokenType.LParen, "Отсутствует токен начала условного выражения '('.");

        Consume(TokenType.Let, "Отсутствует токен объявления индексатора 'let'.");
        IStatement indexatorDeclaration = ParseVariableDeclaration(true);
        Consume(TokenType.Colon, "Отсутствует токен разделения параметров цикла for ':'.");
        IExpression condition = ParseExpression();
        Consume(TokenType.Colon, "Отсутствует токен разделения параметров цикла for ':'.");
        Consume(TokenType.Identifier, "Отсутствует токен идентификатора для определения поведения итератора.");
        IStatement iterator = ParseAssignment(true);
        
        Consume(TokenType.RParen, "Отсутствует токен окончания условного выражения ')'.");

        IStatement block = ParseStatementOrBlock();

        return new ForLoopStatement(variableStorage, indexatorDeclaration, condition, iterator, block);
    }

    private IStatement ParseBreakStatement()
    {
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new BreakStatement();
    }

    private IStatement ParseContinueStatement()
    {
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new ContinueStatement();
    }

    private IStatement ParseReturnStatement()
    {
        IExpression? expr = null;
        if (Peek().Type != TokenType.Semicolon) expr = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");
        return new ReturnStatement(expr);
    }

    private IStatement ParseStatementOrBlock()
    {
        variableStorage.EnterScope();
        if (Peek().Type != TokenType.LBrace) return ParseStatement();
        Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");
        List<IStatement> block = [];
        while (Peek().Type != TokenType.RBrace) block.Add(ParseStatement());
        Consume(TokenType.RBrace, "Отсутствует токен окончания блока операторов '}'.");
        variableStorage.ExitScope();
        return new BlockStatement(block);
    }

    private IExpression ParseExpression() => ParseLogicalOr();

    private IExpression ParseLogicalOr()
    {
        IExpression expr = ParseLogicalAnd();

        Token token = Peek();
        while (Match(TokenType.Or))
        {
            IExpression right = ParseLogicalAnd();
            expr = new BinaryExpression(new Token(TokenType.Or, "||"), expr, right);
        }

        return expr;
    }

    private IExpression ParseLogicalAnd()
    {
        IExpression expr = ParseEquality();

        while (Match(TokenType.And))
        {
            IExpression right = ParseEquality();
            expr = new BinaryExpression(new Token(TokenType.And, "&&"), expr, right);
        }

        return expr;
    }

    private IExpression ParseEquality()
    {
        IExpression expr = ParseComparison();

        while (true)
        {
            if (Match(TokenType.Equals)) expr = new BinaryExpression(new Token(TokenType.Equals, "=="), expr, ParseComparison());
            else if (Match(TokenType.NotEquals)) expr = new BinaryExpression(new Token(TokenType.NotEquals, "!="), expr, ParseComparison());
            else break;
        }

        return expr;
    }

    private IExpression ParseComparison()
    {
        IExpression expr = ParseAdditive();

        while (true)
        {
            if (Match(TokenType.Greater)) expr = new BinaryExpression(new Token(TokenType.Greater, ">"), expr, ParseAdditive());
            else if (Match(TokenType.GreaterEqual)) expr = new BinaryExpression(new Token(TokenType.GreaterEqual, ">="), expr, ParseAdditive());
            else if (Match(TokenType.Less)) expr = new BinaryExpression(new Token(TokenType.Less, "<"), expr, ParseAdditive());
            else if (Match(TokenType.LessEqual)) expr = new BinaryExpression(new Token(TokenType.LessEqual, "<="), expr, ParseAdditive());
            else break;
        }

        return expr;
    }

    private IExpression ParseAdditive()
    {
        IExpression expr = ParseMultiplicative();

        while (true)
        {
            if (Match(TokenType.Plus)) expr = new BinaryExpression(new Token(TokenType.Plus, "+"), expr, ParseMultiplicative());
            else if (Match(TokenType.Minus)) expr = new BinaryExpression(new Token(TokenType.Minus, "-"), expr, ParseMultiplicative());
            else break;
        }

        return expr;
    }

    private IExpression ParseMultiplicative()
    {
        IExpression expr = ParseUnary();

        while (true)
        {
            if (Match(TokenType.Multiply)) expr = new BinaryExpression(new Token(TokenType.Multiply, "*"), expr, ParseUnary());
            else if (Match(TokenType.Divide)) expr = new BinaryExpression(new Token(TokenType.Divide, "/"), expr, ParseUnary());
            else if (Match(TokenType.Modulo)) expr = new BinaryExpression(new Token(TokenType.Modulo, "%"), expr, ParseUnary());
            else break;
        }

        return expr;
    }

    private IExpression ParseUnary()
    {
        if (Match(TokenType.Minus)) return new UnaryExpression(new Token(TokenType.Minus, "-"), ParsePrimary());
        if (Match(TokenType.Not)) return new UnaryExpression(new Token(TokenType.Not, "!"), ParsePrimary());

        return ParsePrimary();
    }

    private IExpression ParsePrimary()
    {
        if (Match(TokenType.LParen))
        {
            IExpression expr = ParseExpression();
            Consume(TokenType.RParen, "Отсутствует токен закрывающей скобки ')'.");
            return expr;
        }

        Token token = Peek();
        switch (token.Type)
        {
            case TokenType.IntLiteral:
                pos++;
                return new LiteralExpression(new IntValue(int.Parse(token.Value)));
            case TokenType.FloatLiteral:
                pos++;
                return new LiteralExpression(new FloatValue(float.Parse(token.Value.Replace(".", ","))));
            case TokenType.DoubleLiteral:
                pos++;
                return new LiteralExpression(new DoubleValue(double.Parse(token.Value.Replace(".", ","))));
            case TokenType.DecimalLiteral:
                pos++;
                return new LiteralExpression(new DecimalValue(decimal.Parse(token.Value.Replace(".", ","))));
            case TokenType.StringLiteral:
                pos++;
                return new LiteralExpression(new StringValue(token.Value));
            case TokenType.BooleanLiteral:
                pos++;
                return new LiteralExpression(new BoolValue(bool.Parse(token.Value)));
            case TokenType.Identifier:
                pos++;

                if (Peek().Type == TokenType.LParen) return ParseFunctionCall(token.Value);
                if (Match(TokenType.Dot))
                {
                    Token memberName = Consume(TokenType.Identifier, $"Отсутствует токен идентификатора члена класса {token.Value}.");
                    return new MemberAccessExpression(new VariableExpression(variableStorage, token.Value), memberName.Value);
                }

                return new VariableExpression(variableStorage, token.Value);
            case TokenType.New:
                pos++;
                Token className = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
                Consume(TokenType.LParen, "Отсутствует токен начала перечисления параметров конструктора '('.");
                Consume(TokenType.RParen, "Отсутствует токен окончания перечисления параметров конструктора ')'.");
                return new NewExpression(classStorage, className.Value);
            default:
                throw new Exception($"Неожиданный токен: '{token}'.");
        }
    }

    private IExpression ParseFunctionCall(string functionName)
    {
        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        List<IExpression> args = [];

        while (!Match(TokenType.RParen))
        {
            if (args.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");
            args.Add(ParseExpression());
        }

        return new FunctionCallExpression(functionStorage, functionName, args);
    }

    public static bool IsTypeCompatible(TypeValue left, TypeValue right) => left switch
    {
        TypeValue.Int => right == TypeValue.Int,
        TypeValue.Float => right switch
        {
            TypeValue.Int or TypeValue.Float => true,
            _ => false
        },
        TypeValue.Double => right switch
        {
            TypeValue.Int or TypeValue.Float or TypeValue.Double => true,
            _ => false
        },
        TypeValue.Decimal => right switch
        {
            TypeValue.Int or TypeValue.Float
            or TypeValue.Double or TypeValue.Decimal => true,
            _ => false
        },
        TypeValue.String => right == TypeValue.String,
        TypeValue.Bool => right == TypeValue.Bool,
        TypeValue.Class => right == TypeValue.Class,
        TypeValue.Void => false,
        _ => false
    };

    private Token GetTypeToken()
    {
        Token current = Peek();

        return current.Type switch
        {
            TokenType.Int or TokenType.Float or TokenType.Double or TokenType.Decimal or TokenType.String or TokenType.Bool or TokenType.Identifier => current,
            _ => throw new Exception($"Текущий токен не является типом ({current.Type}).")
        };
    }

    private TypeValue GetTypeValueFromToken(TokenType type) => type switch
    {
        TokenType.Int => TypeValue.Int,
        TokenType.Float => TypeValue.Float,
        TokenType.Double => TypeValue.Double,
        TokenType.Decimal => TypeValue.Decimal,
        TokenType.String => TypeValue.String,
        TokenType.Bool => TypeValue.Bool,
        TokenType.Identifier => TypeValue.Class,
        _ => throw new Exception($"Не удается получить тип переменной/поля ({type}).")
    };

    private Token Peek(int relativePos = 0) => pos + relativePos < tokens.Count ? tokens[pos + relativePos] : throw new IndexOutOfRangeException();

    private bool Match(TokenType type)
    {
        if (Peek().Type == type)
        {
            pos++;
            return true;
        }

        return false;
    }

    private Token Consume(TokenType type, string errorMessage)
    {
        if (Peek().Type == type) return tokens[pos++];

        throw new Exception(errorMessage);
    }
}