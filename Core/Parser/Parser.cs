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
        _ => throw new Exception($"Не удается получить тип переменной/поля.")
    };

    private Token Peek(int relativePos = 0) => pos + relativePos < tokens.Count ? tokens[pos] : throw new IndexOutOfRangeException();

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
        if (Match(type)) return Peek(-1);

        throw new Exception(errorMessage);
    }
}