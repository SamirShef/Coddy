namespace Core.Lexer;

public enum TokenType
{
    Identifier,

    Class, Let, Func, If, Else, While, Do, For, Break, Continue, Return, Include, Use, Throw, Try, Catch, Finally, Switch, Const,

    Private, Public, New, This, Constructor, Static, Virtual, Override, Enum, Interface, Getter, Setter, Lambda, Parent,

    IntLiteral, FloatLiteral, DoubleLiteral, DecimalLiteral, StringLiteral, BooleanLiteral,

    Plus, Minus, Multiply, Divide, Modulo, Assign,
    PlusAssign, MinusAssign, MultiplyAssign, DivideAssign, ModuloAssign,
    Increment, Decrement, Greater, Less, GreaterEqual, LessEqual,
    Equals, Not, NotEquals, LeftShift, RightShift, LogicalRightShift,

    And, Or,

    LBrace, RBrace, LParen, RParen, LBracket, RBracket,
    
    Semicolon, Comma, Dot, Question, Colon, Implementation,
    
    EOF
}
