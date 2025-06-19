using Core.AST.Statements;
using Core.Expressions;
using Core.Runtime.Functions;
using Core.Values;
using Core.Lexer;
using Core.Runtime.OOP;

namespace Core.Parser;

public class Parser (List<Token> tokens)
{
    private readonly List<Token> tokens = tokens;
    private int pos;

    public List<IStatement> Parse()
    {
        List<IStatement> statements = [];

        while (Peek().Type != TokenType.EOF) statements.Add(ParseStatement());

        return statements;
    }

    private IStatement ParseStatement()
    {
        if (Match(TokenType.Include)) return ParseIncludeStatement();
        if (Match(TokenType.Use)) return ParseUseStatement();
        if (Match(TokenType.Static) || Match(TokenType.Class)) return ParseClassDeclarationStatement();
        if (Match(TokenType.Let)) return ParseVariableDeclarationStatement();
        if (Match(TokenType.Identifier) || Match(TokenType.This))
        {
            if (Peek().Type == TokenType.Dot)
            {
                IExpression target = new VariableExpression(Peek(-1).Value);
                return ParseMethodCallOrFieldAssignment(target);
            }
            if (Match(TokenType.LParen)) return ParseFunctionCallStatement();
            return ParseAssignmentStatement();
        }
        if (Match(TokenType.Enum)) return ParseEnumDeclarationStatement();
        if (Match(TokenType.Interface)) return ParseInterfaceDeclarationStatement();
        if (Match(TokenType.Func)) return ParseFunctionDeclarationStatement();
        if (Match(TokenType.Switch)) return ParseSwitchStatement();
        if (Match(TokenType.If)) return ParseIfElseStatement();
        if (Match(TokenType.While)) return ParseWhileLoopStatement();
        if (Match(TokenType.Do)) return ParseDoWhileLoopStatement();
        if (Match(TokenType.For)) return ParseForLoopStatement();
        if (Match(TokenType.Break)) return ParseBreakStatement();
        if (Match(TokenType.Continue)) return ParseContinueStatement();
        if (Match(TokenType.Return)) return ParseReturnStatement();
        if (Match(TokenType.Throw)) return ParseThrowStatement();
        if (Match(TokenType.Try)) return ParseTryCatchFinalyStatement();

        throw new Exception($"Неожиданный токен: {Peek()}.");
    }

    private IStatement ParseIncludeStatement()
    {
        IExpression libraryPathExpression = ParseExpression();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new IncludeStatement(libraryPathExpression);
    }

    private IStatement ParseUseStatement()
    {
        IExpression filePathExpression = ParseExpression();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new UseStatement(filePathExpression);
    }

    private IStatement ParseClassDeclarationStatement()
    {
        bool classIsStatic = Peek(-1).Type == TokenType.Static;
        if (classIsStatic) pos++;

        Token classNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");

        string? genericsParameters = null;
        if (Match(TokenType.Less)) genericsParameters = ParseGenericsParameters();

        List<string> implements = [];
        if (Match(TokenType.Implementation))
        {
            while (Peek().Type != TokenType.LBrace)
            {
                if (implements.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

                List<string> implementNameExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    implementNameExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }
                string implementNameExpression = string.Join(".", implementNameExpressions);

                implements.Add(implementNameExpression);
            }
        }

        Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");

        ClassInfo classInfo = new(classNameToken.Value)
        {
            GenericsParameters = genericsParameters,
            Implements = implements,
            IsStatic = classIsStatic
        };

        List<IStatement> statements = [];

        while (!Match(TokenType.RBrace))
        {
            if (Match(TokenType.Constructor)) statements.Add(ParseConstructorDeclaration(classInfo));
            else
            {
                Token accessToken = Peek(); pos++;
                AccessModifier access = AccessModifier.Private;
                if (accessToken.Type == TokenType.Private) access = AccessModifier.Private;
                else if (accessToken.Type == TokenType.Public) access = AccessModifier.Public;
                else pos--;
                
                if (Match(TokenType.Enum))
                {
                    statements.Add(ParseClassEnumDeclarationStatement(access));
                    continue;
                }
                else if (Match(TokenType.Interface))
                {
                    statements.Add(ParseClassInterfaceDeclarationStatement(access));
                    continue;
                }
                List<string> modifiers = [];
                while (Peek().Type != TokenType.Func && Peek().Type != TokenType.Let)
                {
                    if (Match(TokenType.Static)) modifiers.Add("static");
                    else if (Match(TokenType.Virtual)) modifiers.Add("virtual");
                    else if (Match(TokenType.Override)) modifiers.Add("override");
                    else throw new Exception($"Токен '{Peek().Value}' не является модификатором.");
                }
                if (Peek().Type == TokenType.Func) statements.Add(ParseMethodDeclarationStatement(access, classInfo, modifiers));
                else statements.Add(ParseFieldDeclarationStatement(access, classInfo, modifiers));
            }
        }

        return new ClassDeclarationStatement(classInfo, statements);
    }

    private IStatement ParseConstructorDeclaration(ClassInfo classInfo)
    {
        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        List<(string, string)> parameters = [];

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token parameterName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            List<string> paramTypeExpressions = [];
            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.LBracket))
                {
                    typeExpression += "[]";
                    pos++;
                }
                paramTypeExpressions.Add(typeExpression);

                if (Match(TokenType.Dot)) continue;
            }
            string paramTypeExpression = string.Join(".", paramTypeExpressions);

            parameters.Add((parameterName.Value, paramTypeExpression));
        }

        List<IExpression> parentParameters = [];
        if (Match(TokenType.Colon))
        {
            Consume(TokenType.Parent, "Отсутствует токен вызова базового конструктора 'parent'.");
            Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");

            while (!Match(TokenType.RParen)) parentParameters.Add(ParseExpression());
        }

        IStatement body;
        if (Match(TokenType.Lambda)) body = ParseLambdaExpressionStatement();
        else body = ParseStatementOrBlock();

        UserFunction constructor = new("constructor", "void", parameters, body);

        return new ConstructorDeclarationStatement(classInfo, constructor, parentParameters);
    }

    private IStatement ParseFieldDeclarationStatement(AccessModifier access, ClassInfo classInfo, List<string> modifiers)
    {
        Consume(TokenType.Let, "Отсутствует токен объявления поля 'let'.");
        Token fieldNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");

        bool hasGetter = false;
        bool hasSetter = false;
        
        if (Match(TokenType.LParen))
        {
            while (!Match(TokenType.RParen))
            {
                if (Match(TokenType.Getter)) hasGetter = true;
                else if (Match(TokenType.Setter)) hasSetter = true;
                else if (hasGetter && hasSetter) throw new Exception("Поле может иметь до двух свойств (getter и/или setter).");
                else throw new Exception("Отсутствует токен свойства (getter или setter).");

                if (Peek().Type != TokenType.RParen) Consume(TokenType.Comma, "Отсутствует токен перечисления свойств ','.");
            }
        }

        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");

        List<string> typeExpressions = [];
        while (Peek().Type == TokenType.Identifier)
        {
            string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
            if (typeExpression == "boolean") typeExpression = "bool";
            if (Match(TokenType.Less)) typeExpression += $"<{ParseGenericsParameters()}>";
            typeExpressions.Add(typeExpression);

            if (Match(TokenType.Dot)) continue;
        }
        
        if (Match(TokenType.LBracket)) return ParseFieldArrayDeclarationStatement(classInfo, fieldNameToken.Value, typeExpressions, access, modifiers, hasGetter, hasSetter);

        IExpression? initialExpression = null;
        if (Match(TokenType.Assign)) initialExpression = ParseExpression();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new FieldDeclarationStatement(fieldNameToken.Value, typeExpressions, access, initialExpression, modifiers, hasGetter, hasSetter);
    }

    private IStatement ParseFieldArrayDeclarationStatement(ClassInfo classInfo, string name, List<string> typeExpressions, AccessModifier access, List<string> modifiers, bool hasGetter, bool hasSetter)
    {
        Token primaryTypeToken = Peek(-2);
        IExpression? size = null;
        if (Peek().Type != TokenType.RBracket) size = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");

        IExpression? expression = null;
        if (Match(TokenType.Assign)) expression = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new FieldArrayDeclarationStatement(name, typeExpressions, access, size, modifiers, hasGetter, hasSetter, expression);
    }

    private IStatement ParseMethodDeclarationStatement(AccessModifier access, ClassInfo classInfo, List<string> modifiers)
    {
        bool isConstructor = Match(TokenType.Constructor);
        if (!isConstructor) Consume(TokenType.Func, "Отсутствует токен объявления метода 'func'.");

        Token methodNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");

        string? genericsParameters = null;
        if (Match(TokenType.Less)) genericsParameters = ParseGenericsParameters();

        string returnType = "void";

        if (!isConstructor && Match(TokenType.Colon))
        {
            List<string> typeExpressions = [];

            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.LBracket))
                {
                    typeExpression += "[]";
                    pos++;
                }
                typeExpressions.Add(typeExpression);
                if (Match(TokenType.Dot)) continue;
            }

            returnType = string.Join(".", typeExpressions);
        }

        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        List<(string, string)> parameters = [];

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            List<string> paramTypeExpressions = [];
            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.LBracket))
                {
                    typeExpression += "[]";
                    pos++;
                }
                paramTypeExpressions.Add(typeExpression);
                if (Match(TokenType.Dot)) continue;
            }
            string paramTypeExpression = string.Join(".", paramTypeExpressions);

            parameters.Add((paramName.Value, paramTypeExpression));
        }

        IStatement body;
        if (Match(TokenType.Lambda)) body = ParseLambdaExpressionStatement();
        else body = ParseStatementOrBlock();

        UserFunction method = new(methodNameToken.Value, returnType, parameters, body, genericsParameters);

        if (isConstructor)
        {
            classInfo.SetConstructor(method);
            return new MethodDeclarationStatement(methodNameToken.Value, method, access, modifiers);
        }

        return new MethodDeclarationStatement(methodNameToken.Value, method, access, modifiers);
    }

    private IStatement ParseClassEnumDeclarationStatement(AccessModifier access)
    {
        string name = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
        List<EnumMember> members = [];
        Consume(TokenType.LBrace, "Отсутствует токен начала перечисления элементов перечисления '{'.");
        while (!Match(TokenType.RBrace))
        {
            Token token = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            IExpression? expression = null;
            if (Match(TokenType.Assign)) expression = ParseExpression();
            if (Peek().Type == TokenType.Comma)
            {
                if (Peek(1).Type == TokenType.RBrace || Peek(1).Type == TokenType.Identifier) Consume(TokenType.Comma, "Отсутствует токен разделения элементов перечисления ','.");
            }
            members.Add(new EnumMember(token.Value, expression));
        }

        return new ClassEnumDeclarationStatement(access, name, members);
    }

    private IStatement ParseClassInterfaceDeclarationStatement(AccessModifier access)
    {
        string name = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;

        List<string> implements = [];
        if (Match(TokenType.Implementation))
        {
            while (Peek().Type != TokenType.LBrace)
            {
                if (implements.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

                List<string> implementNameExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    implementNameExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }
                string implementNameExpression = string.Join(".", implementNameExpressions);

                implements.Add(implementNameExpression);
            }
        }

        Consume(TokenType.LBrace, "Отсутствует токен начала перечисления методов интерфейса '{'.");

        List<(string, string, List<(string, string)>)> methods = [];

        while (!Match(TokenType.RBrace))
        {
            Consume(TokenType.Func, "Отсутствует токен начала объявления метода 'func'.");
            string methodName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
            string returnType = "void";

            if (Match(TokenType.Colon))
            {
                List<string> typeExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    if (typeExpression == "boolean") typeExpression = "bool";
                    if (Match(TokenType.LBracket))
                    {
                        typeExpression += "[]";
                        pos++;
                    }
                    typeExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }

                returnType = string.Join(".", typeExpressions);
            }

            Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
            List<(string, string)> parameters = [];

            while (!Match(TokenType.RParen))
            {
                if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

                Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
                Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
                List<string> paramTypeExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    if (typeExpression == "boolean") typeExpression = "bool";
                    if (Match(TokenType.LBracket))
                    {
                        typeExpression += "[]";
                        pos++;
                    }
                    paramTypeExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }
                string paramTypeExpression = string.Join(".", paramTypeExpressions);

                parameters.Add((paramName.Value, paramTypeExpression));
            }

            Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            methods.Add((methodName, returnType, parameters));
        }

        return new ClassInterfaceDeclarationStatement(access, name, implements, methods);
    }

    private IStatement ParseVariableDeclarationStatement(bool fromForStatement = false)
    {
        Token identifier = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        string identifierName = identifier.Value;
        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
        List<string> typeExpressions = [];
        while (Peek().Type == TokenType.Identifier)
        {
            string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
            if (typeExpression == "boolean") typeExpression = "bool";
            if (Match(TokenType.Less)) typeExpression += $"<{ParseGenericsParameters()}>";
            
            typeExpressions.Add(typeExpression);

            if (Match(TokenType.Dot)) continue;
        }
        
        if (Match(TokenType.LBracket)) return ParseArrayDeclarationStatement(identifierName, typeExpressions);

        IExpression? expression = null;
        if (Match(TokenType.Assign)) expression = ParseExpression();
        if (expression != null) expression = ParseFieldChain(expression);
        if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new VariableDeclarationStatement(identifierName, typeExpressions, expression);
    }

    private IStatement ParseArrayDeclarationStatement(string name, List<string> typeExpressions)
    {
        IExpression? size = null;
        if (Peek().Type != TokenType.RBracket) size = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");

        IExpression? expression = null;
        if (Match(TokenType.Assign)) expression = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new ArrayDeclarationStatement(name, size, typeExpressions, expression);
    }

    private IStatement ParseFunctionCallStatement()
    {
        Token identifier = Peek(-2);
        List<IExpression> args = ParseArguments();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");
        return new FunctionCallStatement(identifier.Value, args);
    }

    private List<IExpression> ParseArguments()
    {
        List<IExpression> args = [];
        while (!Match(TokenType.RParen))
        {
            if (args.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");
            
            IExpression expr = ParseExpression();
            expr = ParseFieldChain(expr);
            args.Add(expr);
        }

        return args;
    }

    private IStatement ParseAssignmentStatement(bool fromForStatement = false)
    {
        string identifierName = Peek(-1).Value;

        if (Peek().Type == TokenType.Dot)
        {
            IExpression target = new VariableExpression(identifierName);
            return ParseFieldAssignmentStatement(target, target, fromForStatement);
        }

        if (Match(TokenType.LBracket)) return ParseArrayAssignmentStatement(identifierName);

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

        return new AssignmentStatement(identifierName, expression);
    }

    private IStatement ParseArrayAssignmentStatement(string name)
    {
        IExpression index = ParseExpression();
        Consume(TokenType.RBracket, "");

        IExpression expression;

        if (Match(TokenType.PlusAssign) || Match(TokenType.MinusAssign)
        || Match(TokenType.MultiplyAssign) || Match(TokenType.DivideAssign)
        || Match(TokenType.ModuloAssign) || Match(TokenType.Increment)
        || Match(TokenType.Decrement))
        {
            Token opToken = Peek(-1);

            if (opToken.Type == TokenType.Increment || opToken.Type == TokenType.Decrement) expression = new LiteralExpression(new IntValue(1));
            else expression = ParseExpression();

            Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            return CreateCompoundAssignment(name, opToken, expression, true, index);
        }

        Consume(TokenType.Assign, "Отсутствует токен оператора присвоения.");

        expression = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new ArrayAssignmentStatement(name, index, expression);
    }

    private IStatement ParseFieldAssignmentStatement(IExpression start, IExpression target, bool fromForStatement)
    {
        while (Match(TokenType.Dot))
        {
            Token fieldToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            target = new FieldExpression(target, fieldToken.Value, start);
        }

        if (Match(TokenType.PlusAssign) || Match(TokenType.MinusAssign)
        || Match(TokenType.MultiplyAssign) || Match(TokenType.DivideAssign)
        || Match(TokenType.ModuloAssign) || Match(TokenType.Increment)
        || Match(TokenType.Decrement))
        {
            Token opToken = Peek(-1);
            IExpression rightExpr;

            if (opToken.Type == TokenType.Increment || opToken.Type == TokenType.Decrement) rightExpr = new LiteralExpression(new IntValue(1));
            else rightExpr = ParseExpression();

            TokenType opType = opToken.Type switch
            {
                TokenType.PlusAssign => TokenType.Plus,
                TokenType.MinusAssign => TokenType.Minus,
                TokenType.MultiplyAssign => TokenType.Multiply,
                TokenType.DivideAssign => TokenType.Divide,
                TokenType.ModuloAssign => TokenType.Modulo,
                TokenType.Increment => TokenType.Plus,
                TokenType.Decrement => TokenType.Minus,
                _ => throw new Exception($"Неподдерживаемый оператор: {opToken.Value}")
            };

            if (target is not FieldExpression fieldExpr) throw new Exception("Невозможно присвоить новое значение полю: объект не является полем.");

            IExpression currentValue = new FieldExpression(fieldExpr.Target, fieldExpr.Name, start);
            BinaryExpression compoundExpr = new(new Token(opType, opToken.Value), currentValue, rightExpr);

            IExpression assignmentExpr = compoundExpr;
            if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            return new FieldAssignmentStatement(start, fieldExpr.Target, fieldExpr.Name, opToken, assignmentExpr);
        }
        else
        {
            Consume(TokenType.Assign, "Отсутствует токен оператора присвоения.");
            IExpression expression = ParseExpression();
            if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            if (target is not FieldExpression fieldExpr) throw new Exception("Невозможно присвоить новое значение полю: объект не является полем.");

            return new FieldAssignmentStatement(start, fieldExpr.Target, fieldExpr.Name, new Token(TokenType.Assign, "="), expression);
        }
    }

    private IStatement CreateCompoundAssignment(string varName, Token opToken, IExpression rightExpr, bool isArray = false, IExpression? index = null)
    {
        IExpression varExpr = isArray && index != null ? new ArrayExpression(varName, index) : new VariableExpression(varName);

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

        return isArray && index != null ? new ArrayAssignmentStatement(varName, index, compoundExpr) : new AssignmentStatement(varName, compoundExpr);
    }

    private IStatement ParseEnumDeclarationStatement()
    {
        string name = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
        List<EnumMember> members = [];
        Consume(TokenType.LBrace, "Отсутствует токен начала перечисления элементов перечисления '{'.");
        while (!Match(TokenType.RBrace))
        {
            Token token = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            IExpression? expression = null;
            if (Match(TokenType.Assign)) expression = ParseExpression();
            if (Peek().Type == TokenType.Comma)
            {
                if (Peek(1).Type == TokenType.RBrace || Peek(1).Type == TokenType.Identifier) Consume(TokenType.Comma, "Отсутствует токен разделения элементов перечисления ','.");
            }
            members.Add(new EnumMember(token.Value, expression));
        }

        return new EnumDeclarationStatement(name, members);
    }

    public IStatement ParseInterfaceDeclarationStatement()
    {
        string name = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;

        List<string> implements = [];
        if (Match(TokenType.Implementation))
        {
            while (Peek().Type != TokenType.LBrace)
            {
                if (implements.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

                List<string> implementNameExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    implementNameExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }
                string implementNameExpression = string.Join(".", implementNameExpressions);

                implements.Add(implementNameExpression);
            }
        }

        Consume(TokenType.LBrace, "Отсутствует токен начала перечисления методов интерфейса '{'.");

        List<(string, string, List<(string, string)>)> methods = [];

        while (!Match(TokenType.RBrace))
        {
            Consume(TokenType.Func, "Отсутствует токен начала объявления метода 'func'.");
            string methodName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
            string returnType = "void";

            if (Match(TokenType.Colon))
            {
                List<string> typeExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    if (typeExpression == "boolean") typeExpression = "bool";
                    if (Match(TokenType.LBracket))
                    {
                        typeExpression += "[]";
                        pos++;
                    }
                    typeExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }

                returnType = string.Join(".", typeExpressions);
            }

            Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
            List<(string, string)> parameters = [];

            while (!Match(TokenType.RParen))
            {
                if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

                Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
                Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
                List<string> paramTypeExpressions = [];
                while (Peek().Type == TokenType.Identifier)
                {
                    string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                    if (typeExpression == "boolean") typeExpression = "bool";
                    if (Match(TokenType.LBracket))
                    {
                        typeExpression += "[]";
                        pos++;
                    }
                    paramTypeExpressions.Add(typeExpression);
                    if (Match(TokenType.Dot)) continue;
                }
                string paramTypeExpression = string.Join(".", paramTypeExpressions);

                parameters.Add((paramName.Value, paramTypeExpression));
            }

            Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            methods.Add((methodName, returnType, parameters));
        }

        return new InterfaceDeclarationStatement(name, implements, methods);
    }

    private IStatement ParseFunctionDeclarationStatement()
    {
        Token nameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");

        string? genericsParameters = null;
        if (Match(TokenType.Less)) genericsParameters = ParseGenericsParameters();

        string returnType = "void";

        if (Match(TokenType.Colon))
        {
            List<string> typeExpressions = [];
            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.LBracket))
                {
                    typeExpression += "[]";
                    pos++;
                }
                typeExpressions.Add(typeExpression);
                if (Match(TokenType.Dot)) continue;
            }

            returnType = string.Join(".", typeExpressions);
        }

        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        List<(string, string)> parameters = [];

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            List<string> paramTypeExpressions = [];
            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.LBracket))
                {
                    typeExpression += "[]";
                    pos++;
                }
                paramTypeExpressions.Add(typeExpression);
                if (Match(TokenType.Dot)) continue;
            }
            string paramTypeExpression = string.Join(".", paramTypeExpressions);

            parameters.Add((paramName.Value, paramTypeExpression));
        }

        IStatement body;
        if (Match(TokenType.Lambda)) body = ParseLambdaExpressionStatement();
        else body = ParseStatementOrBlock();

        UserFunction function = new(nameToken.Value, returnType, parameters, body, genericsParameters);

        return new FunctionDeclarationStatement(nameToken.Value, function);
    }

    private IStatement ParseSwitchStatement()
    {
        Consume(TokenType.LParen, "Отсутствует токен открывающей круглой скобки '('.");
        IExpression expression = ParseExpression();
        Consume(TokenType.RParen, "Отсутствует токен закрывающей круглой скобки ')'.");

        List<(IExpression, IStatement)> cases = [];
        (IExpression, IStatement)? defaultCase = null;
        Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");
        while (!Match(TokenType.RBrace))
        {
            IExpression caseExpression = ParsePrimary();
            Consume(TokenType.Lambda, "Отсутствует токен перехода к блоку операторов '=>'.");
            IStatement caseBlock;
            if (!Match(TokenType.LBrace)) caseBlock = ParseStatement();
            else caseBlock = ParseStatementOrBlock();

            if (caseExpression is VariableExpression ve && ve.Name == "_") defaultCase = (caseExpression, caseBlock);
            else cases.Add((caseExpression, caseBlock));
        }

        return new SwitchStatement(expression, cases, defaultCase);
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
        Consume(TokenType.RParen, $"Отсутствует токен окончания условного выражения ')'.");
        IStatement block = ParseStatementOrBlock();

        return new WhileLoopStatement(condition, block);
    }

    private IStatement ParseDoWhileLoopStatement()
    {
        IStatement block = ParseStatementOrBlock();
        Consume(TokenType.While, "Отсутствует токен начала условной части цикла do/while 'while'.");
        Consume(TokenType.LParen, "Отсутствует токен начала условного выражения '('.");
        IExpression condition = ParseExpression();
        Consume(TokenType.RParen, "Отсутствует токен окончания условного выражения ')'.");
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new DoWhileLoopStatement(condition, block);
    }

    private IStatement ParseForLoopStatement()
    {
        Consume(TokenType.LParen, "Отсутствует токен начала условного выражения '('.");

        IStatement indexatorDeclaration = ParseVariableDeclarationStatement(true);
        Consume(TokenType.Colon, "Отсутствует токен разделения параметров цикла for ':'.");
        IExpression condition = ParseExpression();
        Consume(TokenType.Colon, "Отсутствует токен разделения параметров цикла for ':'.");
        Consume(TokenType.Identifier, "Отсутствует токен идентификатора для определения поведения итератора.");
        IStatement iterator = ParseAssignmentStatement(true);
        
        Consume(TokenType.RParen, "Отсутствует токен окончания условного выражения ')'.");

        IStatement block = ParseStatementOrBlock();

        return new ForLoopStatement(indexatorDeclaration, condition, iterator, block);
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
        if (expr != null) expr = ParseFieldChain(expr);

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");
        return new ReturnStatement(expr);
    }

    private IStatement ParseThrowStatement()
    {
        IExpression expr = ParseExpression();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");
        return new ThrowStatement(expr);
    }

    private IStatement ParseTryCatchFinalyStatement()
    {
        Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");
        List<IStatement> tryBlock = [];
        while (Peek().Type != TokenType.RBrace) tryBlock.Add(ParseStatement());
        Consume(TokenType.RBrace, "Отсутствует токен окончания блока операторов '}'.");

        List<CatchBlock> catchBlocks = [];
        while (Match(TokenType.Catch))
        {
            Consume(TokenType.LParen, "Отсутствует токен начала объявления параметра '('.");
            string paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            List<string> paramTypeExpressions = [];
            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.LBracket))
                {
                    typeExpression += "[]";
                    pos++;
                }
                paramTypeExpressions.Add(typeExpression);

                if (Match(TokenType.Dot)) continue;
            }
            string paramTypeExpression = string.Join(".", paramTypeExpressions);
            Consume(TokenType.RParen, "Отсутствует токен конца объявления параметра ')'.");

            List<IStatement> catchBlock = [];
            Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");
            while(!Match(TokenType.RBrace)) catchBlock.Add(ParseStatement());

            catchBlocks.Add(new CatchBlock(paramName, paramTypeExpression, catchBlock));
        }

        List<IStatement>? finallyBlock = null;
        if (Match(TokenType.Finally))
        {
            finallyBlock = [];
            Consume(TokenType.LBrace, "Отсутствует токен начала блока операторов '{'.");
            while (!Match(TokenType.RBrace)) finallyBlock.Add(ParseStatement());
        }

        return new TryCatchFinallyStatement(tryBlock, catchBlocks, finallyBlock);
    }

    private IStatement ParseStatementOrBlock()
    {
        if (!Match(TokenType.LBrace)) return ParseStatement();

        List<IStatement> block = [];
        while (!Match(TokenType.RBrace)) block.Add(ParseStatement());

        return new BlockStatement(block);
    }

    private IStatement ParseLambdaExpressionStatement(List<(string, string)>? parameters = null)
    {
        IExpression expression = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new LambdaExpressionStatement(parameters, expression);
    }

    private IExpression ParseExpression() => ParseTernaty();

    private IExpression ParseTernaty()
    {
        IExpression condition = ParseLogicalOr();

        if (Match(TokenType.Question))
        {
            IExpression trueExpression = ParseExpression();
            Consume(TokenType.Colon, "Отсутствует разделительный токен между истинным и ложным выражениями конструкции тернарного оператора.");
            IExpression falseExpression = ParseExpression();

            return new TernaryExpression(condition, trueExpression, falseExpression);
        }

        return condition;
    }

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
        IExpression expr = ParseShift();

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

    private IExpression ParseShift()
    {
        IExpression expr = ParseAdditive();

        while (true)
        {
            if (Match(TokenType.LeftShift)) expr = new BinaryExpression(new Token(TokenType.LeftShift, "<<"), expr, ParseAdditive());
            if (Match(TokenType.RightShift)) expr = new BinaryExpression(new Token(TokenType.RightShift, ">>"), expr, ParseAdditive());
            if (Match(TokenType.LogicalRightShift)) expr = new BinaryExpression(new Token(TokenType.LogicalRightShift, ">>>"), expr, ParseAdditive());
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
        if (Match(TokenType.New))
        {
            Token instanceNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");

            string? genericsParameters = null;
            if (Match(TokenType.Less)) genericsParameters = ParseGenericsParameters();

            Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов конструктора '('.");
            
            List<IExpression> args = ParseArguments();

            IExpression expression = new NewClassExpression(instanceNameToken.Value, genericsParameters, args);
            expression = ParseFieldChain(expression);
            return expression;
        }

        if (Match(TokenType.LBracket))
        {
            List<IExpression> expressions = [];
            while (!Match(TokenType.RBracket))
            {
                expressions.Add(ParseExpression());
                if (Peek().Type != TokenType.RBracket) Consume(TokenType.Comma, "Отсутствует токен перечисления элементов инициализатора массива ','.");
            }

            Consume(TokenType.Colon, "Отсутствует разделительный токен между элементами массива и типом ':'.");

            List<string> typeExpressions = [];
            while (Peek().Type == TokenType.Identifier)
            {
                string typeExpression = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value;
                if (typeExpression == "boolean") typeExpression = "bool";
                if (Match(TokenType.Less)) typeExpression += $"<{ParseGenericsParameters()}>";
                typeExpressions.Add(typeExpression);

                if (Match(TokenType.Dot)) continue;
            }

            return new ArrayDeclarationExpression(expressions, string.Join(".", typeExpressions));
        }

        if (Match(TokenType.This) || Match(TokenType.Parent))
        {
            IExpression expression = new VariableExpression(Peek(-1).Value == "this" ? "this" : "base");
            expression = ParseFieldChain(expression);
            return expression;
        }

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
                IExpression expression;

                if (Match(TokenType.LParen)) expression = ParseFunctionCall(token.Value);
                else if (Match(TokenType.LBracket)) expression = ParseArrayExpression(token.Value);
                else expression = new VariableExpression(token.Value);

                expression = ParseFieldChain(expression);
                
                return expression;
            default:
                throw new Exception($"Неожиданный токен: '{token}'.");
        }
    }

    private IExpression ParseFieldChain(IExpression start)
    {
        IExpression expr = start;
        while (Match(TokenType.Dot))
        {
            Token fieldToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");

            if (Match(TokenType.LParen)) expr = new MethodCallExpression(expr, fieldToken.Value, ParseArguments(), start);
            else if (Match(TokenType.LBracket)) expr = ParseArrayFieldExpression(expr, fieldToken.Value);
            else if (Match(TokenType.Assign)) expr = new AssignmentExpression(expr, fieldToken.Value, ParseExpression());
            else expr = new FieldExpression(expr, fieldToken.Value, start);
        }

        return expr;
    }

    private IExpression ParseArrayFieldExpression(IExpression target, string name)
    {
        IExpression index = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");

        return new ArrayFieldExpression(target, name, index);
    }

    private IExpression ParseFunctionCall(string functionName) => new FunctionCallExpression(functionName, ParseArguments());

    private IExpression ParseArrayExpression(string name)
    {
        IExpression index = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");
        return new ArrayExpression(name, index);
    }

    private IStatement ParseMethodCallOrFieldAssignment(IExpression target)
    {
        IExpression start = target;

        while (Match(TokenType.Dot))
        {
            Token fieldNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            if (Match(TokenType.LParen))
            {
                List<IExpression> args = ParseArguments();
                target = new MethodCallExpression(target, fieldNameToken.Value, args);
            }
            else target = new FieldExpression(target, fieldNameToken.Value, start);
        }

        if (Match(TokenType.Semicolon)) return new MethodCallStatement(target);

        return ParseFieldAssignmentStatement(start, target, false);
    }

    private string ParseGenericsParameters()
    {
        List<string> parameters = [];

        while (!Match(TokenType.Greater))
        {
            parameters.Add(Consume(TokenType.Identifier, "Отсутствует токен идентификатора.").Value);
            if (Peek().Type != TokenType.Greater) Consume(TokenType.Comma, "Отсутствует токен перечисления параметров обобщений ','.");
        }

        return string.Join(", ", parameters);
    }

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