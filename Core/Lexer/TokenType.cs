namespace Core.Lexer;

public enum TokenType
{
    Identifier,

    Class, Let, Func, Return,

    Int, Float, Bool, String,
    NumberLiteral, StringLiteral, BooleanLiteral,

    Plus, Minus, Multiply, Divide, Modulo, Assign,
    PlusAssign, MinusAssign, MultiplyAssign, DivideAssign, ModuloAssign,
    Increment, Decrement, Greater, Less, GreaterEqual, LessEqual,
    Equals, Not, NotEquals,

    And, Or,

    LBrace, RBrace, LParen, RParen, LBracket, RBracket,
    
    Semicolon, Comma, Dot, Colon,
    
    EOF
}
