using Core.AST.Statements;
using Core.Expressions;
using Core.Runtime.Functions;
using Core.Runtime;
using Core.Values;
using Core.Lexer;
using Core.Runtime.OOP;
using System.Collections.Generic;
using System.Data.Common;
using System;

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
        if (Match(TokenType.Let)) return ParseVariableDeclarationStatement();
        if (Match(TokenType.Identifier) || Match(TokenType.This))
        {
            if (Peek().Type == TokenType.Dot)
            {
                IExpression target = new VariableExpression(variableStorage, Peek(-1).Value);
                return ParseMethodCallOrFieldAssignment(target);
            }
            if (Match(TokenType.LParen)) return ParseFunctionCallStatement();
            return ParseAssignmentStatement();
        }
        if (Match(TokenType.Func)) return ParseFunctionDeclarationStatement();
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

        ClassInfo classInfo = new(classNameToken.Value);
        List<IStatement> statements = [];

        while (!Match(TokenType.RBrace))
        {
            if (Match(TokenType.Constructor)) statements.Add(ParseConstructorDeclaration(classInfo));
            else if (Peek().Type == TokenType.Private || Peek().Type == TokenType.Public)
            {
                if (Peek(1).Type == TokenType.Func) statements.Add(ParseMethodDeclarationStatement(classInfo));
                else statements.Add(ParseFieldDeclarationStatement(classInfo));
            }
            else
            {
                if (Peek().Type == TokenType.Func) statements.Add(ParseMethodDeclarationStatement(classInfo));
                else statements.Add(ParseFieldDeclarationStatement(classInfo));
            }
        }

        return new ClassDeclarationStatement(classStorage, classInfo, statements);
    }

    private IStatement ParseConstructorDeclaration(ClassInfo classInfo)
    {
        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        var parameters = new List<(string, string, TypeValue)>();

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token parameterName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            Token parameterType = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления параметра конструктора.");

            parameters.Add((parameterName.Value, parameterType.Value, GetTypeValueFromToken(parameterType.Type)));
        }

        IStatement body = ParseStatementOrBlock();

        UserFunction constructor = new("constructor", "void", TypeValue.Void, parameters, body, variableStorage, classInfo);

        return new ConstructorDeclarationStatement(classInfo, constructor);
    }

    private IStatement ParseFieldDeclarationStatement(ClassInfo classInfo)
    {
        AccessModifier access = AccessModifier.Private;
        if (Match(TokenType.Public)) access = AccessModifier.Public;
        else if (Match(TokenType.Private)) access = AccessModifier.Private;

        Consume(TokenType.Let, "Отсутствует токен объявления поля 'let'.");
        Token fieldNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
        Token typeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления поля.");
        if (Match(TokenType.LBracket)) return ParseFieldArrayDeclarationStatement(classInfo, fieldNameToken.Value, typeToken.Value, access);
        TypeValue type = GetTypeValueFromToken(typeToken.Type);

        IExpression? initialExpression = null;
        if (Match(TokenType.Assign)) initialExpression = ParseExpression();
        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new FieldDeclarationStatement(classInfo, fieldNameToken.Value, typeToken.Value, type, access, initialExpression);
    }

    private IStatement ParseFieldArrayDeclarationStatement(ClassInfo classInfo, string name, string typeValue, AccessModifier access)
    {
        Token primatyTypeToken = Peek(-2);
        IExpression? size = null;
        if (Peek().Type != TokenType.RBracket) size = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");

        List<IExpression> expressions = [];
        if (Match(TokenType.Assign))
        {
            if (Match(TokenType.LBracket)) while (!Match(TokenType.RBracket)) if (!Match(TokenType.Comma)) expressions.Add(ParseExpression());
            else expressions.Add(ParseExpression());
        }

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new FieldArrayDeclarationStatement(classInfo, name, typeValue, TypeValue.Array, GetTypeValueFromToken(primatyTypeToken.Type), access, size, expressions);
    }

    private IStatement ParseMethodDeclarationStatement(ClassInfo classInfo)
    {
        AccessModifier access = AccessModifier.Private;
        if (Match(TokenType.Public)) access = AccessModifier.Public;
        else if (Match(TokenType.Private)) access = AccessModifier.Private;

        bool isConstructor = Match(TokenType.Constructor);
        if (!isConstructor) Consume(TokenType.Func, "Отсутствует токен объявления метода 'func'.");

        Token methodNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        TypeValue returnType = TypeValue.Void;

        Token? typeToken = null;
        if (!isConstructor && Match(TokenType.Colon))
        {
            typeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления метода.");
            returnType = GetTypeValueFromToken(typeToken.Type);
        }

        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        var parameters = new List<(string, string, TypeValue)>();

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            Token paramType = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления параметра метода.");

            parameters.Add((paramName.Value, paramType.Value, GetTypeValueFromToken(paramType.Type)));
        }

        IStatement body = ParseStatementOrBlock();

        var method = new UserFunction(methodNameToken.Value, typeToken?.Value ?? "", returnType, parameters, body, variableStorage, classInfo);

        if (isConstructor)
        {
            classInfo.SetConstructor(method);
            return new MethodDeclarationStatement(classInfo, methodNameToken.Value, method, access);
        }

        return new MethodDeclarationStatement(classInfo, methodNameToken.Value, method, access);
    }

    private IStatement ParseVariableDeclarationStatement(bool fromForStatement = false)
    {
        Token identifier = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        string identifierName = identifier.Value;
        Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
        Token baseTypeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления переменной.");
        TypeValue baseType = GetTypeValueFromToken(baseTypeToken.Type);
        
        if (Match(TokenType.LBracket)) return ParseArrayDeclarationStatement(identifierName, baseTypeToken.Value, baseType);

        IExpression? expression = null;
        if (Match(TokenType.Assign)) expression = ParseExpression();
        if (expression != null) expression = ParseFieldChain(expression);
        if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new VariableDeclarationStatement(variableStorage, classStorage, identifierName, baseTypeToken.Value, baseType, expression);
    }

    private IStatement ParseArrayDeclarationStatement(string name, string baseTypeValue, TypeValue baseType)
    {
        IExpression? size = null;
        if (Peek().Type != TokenType.RBracket) size = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");

        List<IExpression> expressions = [];
        if (Match(TokenType.Assign))
        {
            if (Match(TokenType.LBracket))
            {
                Consume(TokenType.LBracket, $"Отсутствует токен открывающей квадратной скобки '[' ({Peek()}).");
                while (!Match(TokenType.RBracket)) if (!Match(TokenType.Comma)) expressions.Add(ParseExpression());
            }
            else expressions.Add(ParseExpression());
        }

        Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

        return new ArrayDeclarationStatement(variableStorage, name, size, baseTypeValue, baseType, expressions);
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
            IExpression target = new VariableExpression(variableStorage, identifierName);
            return ParseFieldAssignmentStatement(target, fromForStatement);
        }

        if (Match(TokenType.LBracket))
        {
            return ParseArrayAssignmentStatement(identifierName);
        }

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

        return new ArrayAssignmentStatement(variableStorage, name, index, expression);
    }

    private IStatement ParseFieldAssignmentStatement(IExpression target, bool fromForStatement)
    {
        while (Match(TokenType.Dot))
        {
            Token fieldToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            target = new FieldExpression(target, fieldToken.Value);
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

            IExpression currentValue = new FieldExpression(fieldExpr.Target, fieldExpr.Name);
            BinaryExpression compoundExpr = new(new Token(opType, opToken.Value), currentValue, rightExpr);

            IExpression assignmentExpr = compoundExpr;
            if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            return new FieldAssignmentStatement(fieldExpr.Target, fieldExpr.Name, opToken, assignmentExpr);
        }
        else
        {
            Consume(TokenType.Assign, "Отсутствует токен оператора присвоения.");
            IExpression expression = ParseExpression();
            if (!fromForStatement) Consume(TokenType.Semicolon, "Отсутствует токен завершения строки ';'.");

            if (target is not FieldExpression fieldExpr) throw new Exception("Невозможно присвоить новое значение полю: объект не является полем.");

            return new FieldAssignmentStatement(fieldExpr.Target, fieldExpr.Name, new Token(TokenType.Assign, "="), expression);
        }
    }

    private IStatement CreateCompoundAssignment(string varName, Token opToken, IExpression rightExpr, bool isArray = false, IExpression? index = null)
    {
        IExpression varExpr = isArray && index != null ? new ArrayExpression(variableStorage, varName, index) : new VariableExpression(variableStorage, varName);

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

        return isArray && index != null ? new ArrayAssignmentStatement(variableStorage, varName, index, compoundExpr) : new AssignmentStatement(variableStorage, varName, compoundExpr);
    }

    private IStatement ParseFunctionDeclarationStatement()
    {
        Token nameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
        TypeValue returnType = TypeValue.Void;
        Token? typeToken = null;

        if (Match(TokenType.Colon))
        {
            typeToken = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления функции.");
            if (Match(TokenType.LBracket)) { Consume(TokenType.RBracket, ""); returnType = TypeValue.Array; }
            else returnType = GetTypeValueFromToken(typeToken.Type);
        }

        Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов '('.");
        var parameters = new List<(string, string, TypeValue)>();

        while (!Match(TokenType.RParen))
        {
            if (parameters.Count > 0) Consume(TokenType.Comma, "Отсутствует токен перечисления аргументов ','.");

            Token paramName = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.Colon, "Отсутствует разделительный токен между идентификатором и типом ':'.");
            Token paramType = Consume(GetTypeToken().Type, "Отсутствует токен типа для объявления параметра функции.");

            parameters.Add((paramName.Value, paramType.Value, GetTypeValueFromToken(paramType.Type)));
        }

        IStatement body = ParseStatementOrBlock();

        var function = new UserFunction(nameToken.Value, typeToken?.Value ?? "", returnType, parameters, body, variableStorage);

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
        Consume(TokenType.RParen, $"Отсутствует токен окончания условного выражения ')'.");
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

        IStatement indexatorDeclaration = ParseVariableDeclarationStatement(true);
        Consume(TokenType.Colon, "Отсутствует токен разделения параметров цикла for ':'.");
        IExpression condition = ParseExpression();
        Consume(TokenType.Colon, "Отсутствует токен разделения параметров цикла for ':'.");
        Consume(TokenType.Identifier, "Отсутствует токен идентификатора для определения поведения итератора.");
        IStatement iterator = ParseAssignmentStatement(true);
        
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
        if (expr != null) expr = ParseFieldChain(expr);

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
        if (Match(TokenType.New))
        {
            Token instanceNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            Consume(TokenType.LParen, "Отсутствует токен начала перечисления аргументов конструктора '('.");
            
            List<IExpression> args = ParseArguments();

            IExpression expression = new NewClassExpression(classStorage, instanceNameToken.Value, args);
            expression = ParseFieldChain(expression);
            return expression;
        }

        if (Match(TokenType.This))
        {
            IExpression expression = new VariableExpression(variableStorage, "this");
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
                else expression = new VariableExpression(variableStorage, token.Value);

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

            if (Match(TokenType.LParen)) expr = new MethodCallExpression(expr, fieldToken.Value, ParseArguments());
            else if (Match(TokenType.LBracket)) expr = ParseArrayFieldExpression(expr, fieldToken.Value);
            else expr = new FieldExpression(expr, fieldToken.Value);
        }

        return expr;
    }

    private IExpression ParseArrayFieldExpression(IExpression target, string name)
    {
        IExpression index = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");

        return new ArrayFieldExpression(target, name, index);
    }

    private IExpression ParseFunctionCall(string functionName)
    {
        return new FunctionCallExpression(functionStorage, functionName, ParseArguments());
    }

    private IExpression ParseArrayExpression(string name)
    {
        IExpression index = ParseExpression();
        Consume(TokenType.RBracket, "Отсутствует токен закрывающей квадратной скобки ']'.");
        return new ArrayExpression(variableStorage, name, index);
    }

    private IStatement ParseMethodCallOrFieldAssignment(IExpression target)
    {
        while (Match(TokenType.Dot))
        {
            Token fieldNameToken = Consume(TokenType.Identifier, "Отсутствует токен идентификатора.");
            if (Match(TokenType.LParen))
            {
                List<IExpression> args = ParseArguments();
                target = new MethodCallExpression(target, fieldNameToken.Value, args);
            }
            else target = new FieldExpression(target, fieldNameToken.Value);
        }

        if (Match(TokenType.Semicolon)) return new MethodCallStatement(target);

        return ParseFieldAssignmentStatement(target, false);
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
        TypeValue.Array => right == TypeValue.Array,
        TypeValue.Void => false,
        _ => false
    };

    public static IValue ConvertValue(IValue value, TypeValue targetType)
    {
        if (value.Type == targetType) return value;

        return (value, targetType) switch
        {
            (IntValue iv, TypeValue.Float) => new FloatValue(iv.AsInt()),
            (IntValue iv, TypeValue.Double) => new DoubleValue(iv.AsInt()),
            (IntValue iv, TypeValue.Decimal) => new DecimalValue(iv.AsInt()),
            (FloatValue fv, TypeValue.Double) => new DoubleValue(fv.AsFloat()),
            (FloatValue fv, TypeValue.Decimal) => new DecimalValue((decimal)fv.AsFloat()),
            (DoubleValue dv, TypeValue.Float) => new FloatValue((float)dv.AsDouble()),
            (DoubleValue dv, TypeValue.Decimal) => new DecimalValue((decimal)dv.AsDouble()),
            _ => throw new Exception($"Нельзя преобразовать {value.Type} в {targetType}")
        };
    }

    public static TypeValue GetCommonType(TypeValue a, TypeValue b)
    {
        var order = new[] { TypeValue.Int, TypeValue.Float, TypeValue.Double, TypeValue.Decimal };
        int aIndex = Array.IndexOf(order, a);
        int bIndex = Array.IndexOf(order, b);

        if (aIndex == -1 || bIndex == -1)
            throw new Exception($"Невозможно найти общий тип для {a} и {b}.");

        return aIndex > bIndex ? a : b;
    }

    private Token GetTypeToken()
    {
        Token current = Peek();

        return current.Type switch
        {
            TokenType.Int or TokenType.Float or TokenType.Double or TokenType.Decimal or TokenType.String or TokenType.Bool or TokenType.Identifier => current,
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
        TokenType.Identifier => TypeValue.Class,
        _ => throw new Exception($"Не удается получить тип переменной ({type}).")
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