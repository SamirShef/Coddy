namespace Core.Lexer;

public enum TokenType
{
    Identifier,

    Class, Let, Func, If, Else, While, Do, For, Break, Continue, Return,

    Private, Public, New, This,

    Int, Float, Double, Decimal, Bool, String,
    IntLiteral, FloatLiteral, DoubleLiteral, DecimalLiteral, StringLiteral, BooleanLiteral,

    Plus, Minus, Multiply, Divide, Modulo, Assign,
    PlusAssign, MinusAssign, MultiplyAssign, DivideAssign, ModuloAssign,
    Increment, Decrement, Greater, Less, GreaterEqual, LessEqual,
    Equals, Not, NotEquals,

    And, Or,

    LBrace, RBrace, LParen, RParen, LBracket, RBracket,
    
    Semicolon, Comma, Dot, Question, Colon,
    
    EOF
}
