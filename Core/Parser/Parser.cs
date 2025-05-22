using Core.AST.Statements;
using Core.Expressions;
using Core.Lexer;
using Core.Runtime;
using Core.Values;

namespace Core.Parser;

public class Parser (List<Token> tokens)
{
    private readonly List<Token> tokens = tokens;
    private int pos;

    private readonly VariableStorage variableStorage = new();

    public List<IStatement> Parse()
    {
        List<IStatement> statements = [];

        while (Peek().Type != TokenType.EOF)
        {
            statements.Add(ParseStatement());
        }

        return statements;
    }

    private IStatement ParseStatement()
    {
        if (Match(TokenType.Let)) return ParseVariableDeclaration();
        if (Match(TokenType.Identifier)) return ParseAssignment();

        throw new Exception($"Неожиданный токен: {Peek()}.");
    }

    private IStatement ParseVariableDeclaration()
    {
        Token identifier = Consume(TokenType.Identifier, "Отсутствует токен идентификатора для объявления переменной/поля.");
        string identifierName = identifier.Value;
        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом переменной/поля.");
        Token baseTypeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления переменной/поля.");
        TypeValue baseType = GetTypeValueFromToken(baseTypeToken.Type);
        IExpression? expression = null;
        if (Match(TokenType.Assign))
        {
            expression = ParseExpression();
        }

        Consume(TokenType.Semicolon, "Отсутствует токен, завершающий строку (';').");
        
        return new VariableDeclarationStatement(variableStorage, identifierName, baseType, expression);
    }

    private IStatement ParseAssignment()
    {
        Token identifier = Peek(-1);
        string identifierName = identifier.Value;

        Consume(TokenType.Assign, "Отсутствует токен присвоения.");

        IExpression expression = ParseExpression();

        Consume(TokenType.Semicolon, "Отсутствует токен, завершающий строку (';').");

        return new AssignmentStatement(variableStorage, identifierName, expression.Evaluate());
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
            if (Match(TokenType.Equals))
                expr = new BinaryExpression(new Token(TokenType.Equals, "=="), expr, ParseComparison());
            else if (Match(TokenType.NotEquals))
                expr = new BinaryExpression(new Token(TokenType.NotEquals, "!="), expr, ParseComparison());
            else
                break;
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
        if (Match(TokenType.Minus))
        {
            return new UnaryExpression(new Token(TokenType.Minus, "-"), ParsePrimary());
        }
        if (Match(TokenType.Not))
        {
            return new UnaryExpression(new Token(TokenType.Not, "!"), ParsePrimary());
        }
        return ParsePrimary();
    }

    private IExpression ParsePrimary()
    {
        if (Match(TokenType.LParen))
        {
            IExpression expr = ParseExpression();
            Consume(TokenType.RParen, "Ожидается закрывающая скобка ')'.");
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
                return new LiteralExpression(new FloatValue(float.Parse(token.Value)));
            case TokenType.DoubleLiteral:
                pos++;
                return new LiteralExpression(new DoubleValue(double.Parse(token.Value)));
            case TokenType.DecimalLiteral:
                pos++;
                return new LiteralExpression(new DecimalValue(decimal.Parse(token.Value)));
            case TokenType.StringLiteral:
                pos++;
                return new LiteralExpression(new StringValue(token.Value));
            case TokenType.BooleanLiteral:
                pos++;
                return new LiteralExpression(new BoolValue(bool.Parse(token.Value)));
            case TokenType.Identifier:
                pos++;
                return new VariableExpression(variableStorage, token.Value);
            default:
                throw new Exception($"Неожиданный токен: '{token}'.");
        }
    }

    private Token GetTypeToken()
    {
        Token current = Peek();

        return current.Type switch
        {
            TokenType.Int or TokenType.Float or TokenType.Double or TokenType.Decimal or TokenType.String or TokenType.Bool => current,
            _ => throw new Exception("Текущий токен не является типом.")
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