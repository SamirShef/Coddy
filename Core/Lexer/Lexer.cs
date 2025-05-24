using System.Text;

namespace Core.Lexer;

public class Lexer (string source)
{
    private readonly string source = source;
    private int pos;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        { "class", TokenType.Class },
        { "let", TokenType.Let },
        { "int", TokenType.Int },
        { "float", TokenType.Float },
        { "double", TokenType.Double },
        { "decimal", TokenType.Decimal },
        { "string", TokenType.String },
        { "boolean", TokenType.Bool },
        { "func", TokenType.Func },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "while", TokenType.While },
        { "do", TokenType.Do },
        { "for", TokenType.For },
        { "break", TokenType.Break },
        { "continue", TokenType.Continue },
        { "return", TokenType.Return },
        { "priv", TokenType.Private },
        { "pub", TokenType.Public },
        { "new", TokenType.New },
    };

    public IEnumerable<Token> Tokenize()
    {
        while (pos < source.Length)
        {
            if (char.IsWhiteSpace(source[pos]))
            {
                pos++;
                continue;
            }

            if (char.IsLetter(source[pos]))
            {
                yield return ReadIdentifierOrKeyword();
                continue;
            }

            if (char.IsDigit(source[pos]) || (source[pos] == '.' && char.IsDigit(source[pos + 1])))
            {
                yield return ReadNumberLiteral();
                continue;
            }

            if (source[pos] == '\"' || source[pos] == '\'')
            {
                yield return ReadStringLiteral(source[pos]);
                continue;
            }

            switch (source[pos])
            {
                case '+':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.PlusAssign, "+=");
                        pos += 2;
                    }
                    else if (source[pos + 1] == '+')
                    {
                        yield return new Token(TokenType.Increment, "++");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Plus, "+");
                        pos++;
                    }

                    break;
                case '-':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.MinusAssign, "-=");
                        pos += 2;
                    }
                    else if (source[pos + 1] == '-')
                    {
                        yield return new Token(TokenType.Decrement, "--");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Minus, "-");
                        pos++;
                    }

                    break;
                case '*':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.MultiplyAssign, "*=");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Multiply, "*");
                        pos++;
                    }

                    break;
                case '/':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.DivideAssign, "/=");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Divide, "/");
                        pos++;
                    }

                    break;
                case '%':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.ModuloAssign, "%=");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Modulo, "%");
                        pos++;
                    }

                    break;
                case '=':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.Equals, "==");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Assign, "=");
                        pos++;
                    }

                    break;
                case '!':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.NotEquals, "!=");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Not, "!");
                        pos++;
                    }

                    break;
                case '>':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.GreaterEqual, ">=");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Greater, ">");
                        pos++;
                    }

                    break;
                case '<':
                    if (source[pos + 1] == '=')
                    {
                        yield return new Token(TokenType.LessEqual, "<=");
                        pos += 2;
                    }
                    else
                    {
                        yield return new Token(TokenType.Less, "<");
                        pos++;
                    }

                    break;
                case '&':
                    if (source[pos + 1] == '&')
                    {
                        yield return new Token(TokenType.And, "&&");
                        pos += 2;
                    }
                    else throw new Exception($"Неизвестный оператор '{source[pos]}'. Возможно, вы имели ввиду '&&'?");
                    break;
                case '|':
                    if (source[pos + 1] == '|')
                    {
                        yield return new Token(TokenType.Or, "||");
                        pos += 2;
                    }
                    else throw new Exception($"Неизвестный оператор '{source[pos]}'. Возможно, вы имели ввиду '||'?");
                    break;
                case '(':
                    yield return new Token(TokenType.LParen, "("); pos++; break;
                case ')':
                    yield return new Token(TokenType.RParen, ")"); pos++; break;
                case '{':
                    yield return new Token(TokenType.LBrace, "{"); pos++; break;
                case '}':
                    yield return new Token(TokenType.RBrace, "}"); pos++; break;
                case '[':
                    yield return new Token(TokenType.LBracket, "["); pos++; break;
                case ']':
                    yield return new Token(TokenType.RBracket, "]"); pos++; break;
                case ':':
                    yield return new Token(TokenType.Colon, ":"); pos++; break;
                case ';':
                    yield return new Token(TokenType.Semicolon, ";"); pos++; break;
                case '.':
                    yield return new Token(TokenType.Dot, "."); pos++; break;
                case ',':
                    yield return new Token(TokenType.Comma, ","); pos++; break;
                default: throw new Exception($"Неизвестный оператор '{source[pos]}'.");
            }
        }

        yield return new Token(TokenType.EOF, "");
    }

    private Token ReadIdentifierOrKeyword()
    {
        StringBuilder builder = new();

        while (char.IsLetterOrDigit(source[pos])) builder.Append(source[pos++]);

        if (keywords.TryGetValue(builder.ToString(), out TokenType type)) return new Token(type, builder.ToString());

        if (builder.ToString() == "true" || builder.ToString() == "false") return new Token(TokenType.BooleanLiteral, builder.ToString());

        return new Token(TokenType.Identifier, builder.ToString());
    }

    private Token ReadNumberLiteral()
    {
        StringBuilder builder = new();
        char suffix = '\0';

        while (pos < source.Length && (char.IsDigit(source[pos]) || source[pos] == '.'))
        {
            if (source[pos] == '.')
            {
                if (builder.ToString().Contains('.')) throw new Exception($"Некорректный формат числа (число содержит две точки): {builder}.");
                else if (builder.Length == 0) builder.Append('0');
            }

            builder.Append(source[pos++]);
        }

        char current = pos < source.Length ? source[pos] : '\0';
        switch (current)
        {
            case 'f': suffix = 'f'; break;
            case 'd': suffix = 'd'; break;
            case 'm': suffix = 'm'; break;
            default: break;
        }

        if (suffix == '\0' && builder.ToString().Contains('.')) throw new Exception($"Некорректный формат числа (число содержит точку, но не имеет суффикса 'f', 'd' или 'm'): {builder}.");
        else if (suffix != '\0') pos++;

        return suffix switch
        {
            'f' => new Token(TokenType.FloatLiteral, builder.ToString()),
            'd' => new Token(TokenType.DoubleLiteral, builder.ToString()),
            'm' => new Token(TokenType.DecimalLiteral, builder.ToString()),
            _ => new Token(TokenType.IntLiteral, builder.ToString())
        };
    }

    private Token ReadStringLiteral(char closeMark)
    {
        StringBuilder builder = new();
        pos++;

        while (source[pos] != closeMark)
        {
            if (source[pos] == '\0') throw new Exception($"Требуется закрывающая кавычка строкового литерала: {closeMark}.");

            builder.Append(source[pos++]);
        }

        pos++;

        return new Token(TokenType.StringLiteral, builder.ToString());
    }
}
