using Core.Lexer;

namespace Core.Parser;

public class Parser (List<Token> tokens)
{
    private readonly List<Token> tokens = tokens;
    private int pos;
}