using Core.AST.Statements;
using Core.Expressions;
using Core.Values;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Core.Translator;

/// <summary>
/// Class to translate Coddy code to C# code
/// </summary>
public class Translator()
{
    public static string Translate(List<IStatement> statements)
    {
        List<IStatement> statementsInMain = [];
        List<IStatement> statementsInGlobalClass = [];

        foreach (IStatement statement in statements)
        {
            if (CodePartInGlobal(statement)) statementsInGlobalClass.Add(statement);
            else statementsInMain.Add(statement);
        }

        StringBuilder builder = new();

        builder.AppendLine("using System;");
        builder.AppendLine("using IDE.Runtime;");
        builder.AppendLine();
        builder.AppendLine("public class Program {");
        builder.AppendLine("public static void __Main__() {");
        foreach (IStatement statement in statementsInMain) builder.AppendLine($"{TranslateCodePart(statement)}");
        builder.AppendLine("}");
        foreach (IStatement statement in statementsInGlobalClass) builder.AppendLine($"{TranslateCodePart(statement)}");
        builder.AppendLine("}");
        
        return builder.ToString();
    }

    private static bool CodePartInGlobal(IStatement statement) => statement switch
    {
        IncludeStatement or ClassDeclarationStatement or FunctionDeclarationStatement => true,
        _ => false
    };

    private static string TranslateCodePart(IStatement statement) => statement switch
    {
        IncludeStatement ins => TranslateIncludeStatement(ins),
        ClassDeclarationStatement cds => TranslateClassDeclarationStatement(cds),
        FieldDeclarationStatement fds => TranslateFieldDeclarationStatement(fds),
        FieldArrayDeclarationStatement fads => TranslateFieldArrayDeclarationStatement(fads),
        FieldAssignmentStatement fas => TranslateFieldAssignmentStatement(fas),
        MethodDeclarationStatement mds => TranslateMethodDeclarationStatement(mds),
        MethodCallStatement mcs => TranslateMethodCallStatement(mcs),
        ConstructorDeclarationStatement cds => TranslateConstructorDeclarationStatement(cds),
        VariableDeclarationStatement vds => TranslateVariableDeclarationStatement(vds),
        ArrayDeclarationStatement ads => TranslateArrayDeclarationStatement(ads),
        ArrayAssignmentStatement aas => TranslateArrayAssignmentStatement(aas),
        AssignmentStatement ags => TranslateAssignmentStatement(ags),
        IfElseStatement ies => TranslateIfElseStatement(ies),
        WhileLoopStatement wls => TranslateWhileStatement(wls),
        DoWhileLoopStatement dwls => TranslateDoWhileStatement(dwls),
        ForLoopStatement fls => TranslateForStatement(fls),
        FunctionDeclarationStatement fds => TranslateFunctionDeclarationStatement(fds),
        FunctionCallStatement fcs => TranslateFunctionCallStatement(fcs),
        ReturnStatement rs => TranslateReturnStatement(rs),
        BreakStatement => "break;",
        ContinueStatement => "continue;",
        _ => throw new Exception($"Невозможно обработать данный тип команды ({statement}).")
    };

    private static string TranslateIncludeStatement(IncludeStatement ins)
    {
        string fullPath = Path.Combine("Libraries", ins.LibraryPath);
        if (!File.Exists(fullPath)) throw new Exception($"Библиотека не найдена: {fullPath}");

        string extension = Path.GetExtension(fullPath).ToLower();
        return extension switch
        {
            ".cd" => TranslateCoddyLibrary(fullPath),
            ".dll" => TranslateDLLLibrary(fullPath),
            ".cs" => TranslateCSharpLibrary(fullPath),
            _ => throw new Exception($"Файл с расширением '{extension}' не является поддерживаемым библиотечным файлом. Поддерживаемые расширения: .cd, .dll, .cs"),
        };
    }

    private static string TranslateCoddyLibrary(string fullPath)
    {
        string source = File.ReadAllText(fullPath);
        Lexer.Lexer lexer = new(source);
        Parser.Parser parser = new([.. lexer.Tokenize()]);
        List<IStatement> statements = parser.Parse();

        StringBuilder builder = new();
        foreach (IStatement statement in statements)
        {
            if (statement is IncludeStatement includeStatement) builder.AppendLine(TranslateIncludeStatement(includeStatement));
            if (statement is ClassDeclarationStatement classDeclaration) builder.AppendLine(TranslateClassDeclarationStatement(classDeclaration));
        }

        return builder.ToString();
    }

    private static string TranslateDLLLibrary(string dllPath)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);
            StringBuilder builder = new();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsPublic || type.IsNestedPublic)
                {
                    builder.AppendLine($"public class {type.Name} {{");
                    
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (!method.IsSpecialName)
                        {
                            string parameters = string.Join(", ", method.GetParameters().Select(p => $"{GetCSTypeName(p.ParameterType)} {p.Name}"));
                            string staticModifier = method.IsStatic ? "static " : "";
                            string returnType = GetCSTypeName(method.ReturnType);
                            string methodName = EscapeCSharpKeyword(method.Name);
                            
                            builder.AppendLine($"public {staticModifier}{returnType} {methodName}({parameters}) {{");
                            
                            var methodBody = method.GetMethodBody();
                            if (methodBody != null)
                            {
                                var ilBytes = methodBody.GetILAsByteArray();
                                if (ilBytes != null && ilBytes.Length > 0)
                                {
                                    if (method.ReturnType == typeof(void))
                                    {
                                        if (method.IsStatic) builder.AppendLine($"{GetFullTypeName(type)}.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                        else builder.AppendLine($"this.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                    }
                                    else
                                    {
                                        if (method.IsStatic) builder.AppendLine($"return {GetFullTypeName(type)}.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                        else builder.AppendLine($"return this.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                    }
                                }
                            }
                            else
                            {
                                if (method.ReturnType == typeof(void))
                                {
                                    if (method.IsStatic) builder.AppendLine($"{GetFullTypeName(type)}.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                    else builder.AppendLine($"this.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                }
                                else
                                {
                                    if (method.IsStatic) builder.AppendLine($"return {GetFullTypeName(type)}.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                    else builder.AppendLine($"return this.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                                }
                            }
                            
                            builder.AppendLine("}");
                        }
                    }

                    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        string staticModifier = field.IsStatic ? "static " : "";
                        string fieldType = GetCSTypeName(field.FieldType);
                        string fieldName = EscapeCSharpKeyword(field.Name);
                        string fieldValue = FormatFieldValue(field.GetValue(null), field.FieldType);
                        
                        builder.AppendLine($"public {staticModifier}{fieldType} {fieldName} = {fieldValue};");
                    }

                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при трансляции DLL библиотеки: {ex.Message}");
        }
    }

    private static string FormatFieldValue(object? value, Type type)
    {
        if (value == null) return "null";

        return type.Name switch
        {
            "Int32" => value.ToString()!,
            "Single" => $"{value.ToString()!.Replace(",", ".")}f",
            "Double" => $"{value.ToString()!.Replace(",", ".")}d",
            "Decimal" => $"{value.ToString()!.Replace(",", ".")}m",
            "String" => $"\"{value}\"",
            "Boolean" => value.ToString()!.ToLower(),
            _ => value.ToString() ?? "null"
        };
    }

    private static string TranslateCSharpLibrary(string csPath)
    {
        try
        {
            string source = File.ReadAllText(csPath);
            return source;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при трансляции C# библиотеки: {ex.Message}");
        }
    }

    private static string GetCSTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string baseName = type.Name.Split('`')[0];
            string[] genericArgs = type.GetGenericArguments().Select(GetCSTypeName).ToArray();
            return $"{baseName}<{string.Join(", ", genericArgs)}>";
        }

        return type.Name switch
        {
            "Int32" => "int",
            "Int64" => "long",
            "Single" => "float",
            "Double" => "double",
            "Decimal" => "decimal",
            "String" => "string",
            "Boolean" => "bool",
            "Void" => "void",
            "Object" => "object",
            _ => type.Name
        };
    }

    private static string TranslateClassDeclarationStatement(ClassDeclarationStatement cds)
    {
        StringBuilder builder = new();
        bool isStatic = cds.ClassInfo.IsStatic;
        string isStaticString = isStatic ? "static" : "";
        string className = EscapeCSharpKeyword(cds.ClassInfo.Name);
        builder.AppendLine($"public {isStaticString} class {className} {{");
        foreach (IStatement statement in cds.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateFieldDeclarationStatement(FieldDeclarationStatement fds)
    {
        StringBuilder builder = new();
        TypeValue type = fds.Type;
        string typeString = type != TypeValue.Class ? type.ToString().ToLower() : EscapeCSharpKeyword(fds.TypeValue);
        bool isStatic = fds.IsStatic;
        string isStaticString = isStatic ? "static" : "";
        string fieldName = EscapeCSharpKeyword(fds.Name);
        builder.Append($"{fds.Access.ToString().ToLower()} {isStaticString} {typeString} {fieldName}");
        if (fds.Expression != null) builder.Append($" = {TranslateExpression(fds.Expression)}");
        builder.Append(';');

        return builder.ToString();
    }

    private static string TranslateFieldArrayDeclarationStatement(FieldArrayDeclarationStatement fads)
    {
        StringBuilder builder = new();

        TypeValue type = fads.Type;
        string typeString = type.ToString().ToLower();
        if (type == TypeValue.Class) typeString = fads.TypeValue;
        if (type == TypeValue.Array) typeString = fads.PrimaryType.ToString().ToLower();

        string sizeString = fads.Size != null ? TranslateExpression(fads.Size) : "";

        builder.Append($"{fads.Access.ToString().ToLower()} {typeString}[] {fads.Name} = new {typeString}[{sizeString}] " + "{ ");

        if (fads.Expressions.Count > 0)
        {
            string[] expressions = new string[fads.Expressions.Count];
            for (int i = 0; i < expressions.Length; i++) expressions[i] = TranslateExpression(fads.Expressions[i]);
            builder.Append(string.Join(", ", expressions));
        }

        builder.Append(" };");

        return builder.ToString();
    }

    private static string TranslateFieldAssignmentStatement(FieldAssignmentStatement fas) => $"{TranslateExpression(fas.TargetExpression)}.{fas.Name} {fas.OpToken.Value} {TranslateExpression(fas.Expression)};";

    private static string TranslateMethodDeclarationStatement(MethodDeclarationStatement mds)
    {
        StringBuilder builder = new();
        string[] parameters = new string[mds.Method.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter(mds.Method.Parameters[i]);
        bool isStatic = mds.IsStatic;
        string isStaticString = isStatic ? "static" : "";
        string methodName = EscapeCSharpKeyword(mds.MethodName);
        builder.AppendLine($"{mds.Access.ToString().ToLower()} {isStaticString} {mds.Method.ReturnType.ToString().ToLower()} {methodName}({string.Join(", ", parameters)}) {{");
        IStatement body = mds.Method.Body;
        if (body is not BlockStatement bs) builder.AppendLine(TranslateCodePart(body));
        else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateMethodCallStatement(MethodCallStatement mcs) => $"{TranslateExpression(mcs.Expression)};";

    private static string TranslateConstructorDeclarationStatement(ConstructorDeclarationStatement cds)
    {
        StringBuilder builder = new();
        string[] parameters = new string[cds.Constructor.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter(cds.Constructor.Parameters[i]);
        builder.AppendLine($"public {cds.ClassInfo.Name}({string.Join(", ", parameters)}) {{");
        IStatement body = cds.Constructor.Body;
        if (body is not BlockStatement bs) builder.AppendLine(TranslateCodePart(body));
        else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateVariableDeclarationStatement(VariableDeclarationStatement vds)
    {
        StringBuilder builder = new();
        
        TypeValue type = vds.Type;
        string typeString = type != TypeValue.Class ? type.ToString().ToLower() : vds.TypeValue;
        
        builder.Append($"{typeString} {vds.Name} = ");
        if (vds.Expression != null) builder.Append($"{TranslateExpression(vds.Expression)}");
        else builder.Append(GetDefaultValueByTypeValue(type));
        builder.Append(';');
        
        return builder.ToString();
    }

    private static string TranslateArrayDeclarationStatement(ArrayDeclarationStatement ads)
    {
        StringBuilder builder = new();

        TypeValue type = ads.Type;
        string typeString = type != TypeValue.Class ? type.ToString().ToLower() : ads.TypeValue;

        string sizeString = ads.Size != null ? TranslateExpression(ads.Size) : "";

        builder.Append($"{typeString}[] {ads.Name} = new {typeString}[{sizeString}] " + "{ ");
        if (ads.Expressions.Count > 0)
        {
            string[] expressions = new string[ads.Expressions.Count];
            for (int i = 0; i < expressions.Length; i++) expressions[i] = TranslateExpression(ads.Expressions[i]);

            builder.Append(string.Join(", ", expressions));
        }

        builder.Append(" };");

        return builder.ToString();
    }

    private static string TranslateArrayAssignmentStatement(ArrayAssignmentStatement aas) => $"{aas.Name}[{TranslateExpression(aas.Index)}] = {TranslateExpression(aas.Expression)};";

    private static string TranslateAssignmentStatement(AssignmentStatement ags) => $"{ags.Name} = {TranslateExpression(ags.NewExpression)};";

    private static string TranslateIfElseStatement(IfElseStatement ies)
    {
        StringBuilder builder = new();
        builder.AppendLine($"if ({TranslateExpression(ies.Condition)}) {{");
        IStatement ifBlock = ies.IfBlock;
        if (ifBlock is not BlockStatement ifBs) builder.AppendLine(TranslateCodePart(ifBlock));
        else foreach (IStatement statement in ifBs.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        IStatement? elseBlock = ies.ElseBlock;
        if (elseBlock != null)
        {
            builder.AppendLine("else {");
            if (elseBlock is not BlockStatement elseBs) builder.AppendLine(TranslateCodePart(elseBlock));
            else foreach (IStatement statement in elseBs.Statements) builder.AppendLine(TranslateCodePart(statement));
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static string TranslateWhileStatement(WhileLoopStatement wls)
    {
        StringBuilder builder = new();
        builder.AppendLine($"while ({TranslateExpression(wls.Condition)}) {{");
        IStatement block = wls.Block;
        if (block is not BlockStatement bs) builder.Append(TranslateCodePart(block));
        else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateDoWhileStatement(DoWhileLoopStatement dwls)
    {
        StringBuilder builder = new();
        builder.AppendLine("do {");
        IStatement block = dwls.Block;
        if (block is not BlockStatement bs) builder.Append(TranslateCodePart(block));
        else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine('}' + $" while ({TranslateExpression(dwls.Condition)});");

        return builder.ToString();
    }

    private static string TranslateForStatement(ForLoopStatement fls)
    {
        StringBuilder builder = new();
        builder.AppendLine($"for ({TranslateCodePart(fls.IndexatorDeclaration)} {TranslateExpression(fls.Condition)}; {TranslateCodePart(fls.Iterator).TrimEnd(';')}) {{");
        IStatement block = fls.Block;
        if (block is not BlockStatement bs) builder.Append(TranslateCodePart(block));
        else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateFunctionDeclarationStatement(FunctionDeclarationStatement fds)
    {
        StringBuilder builder = new();
        
        string[] parameters = new string[fds.UserFunction.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter(fds.UserFunction.Parameters[i]);
        
        TypeValue type = fds.UserFunction.ReturnType;
        string typeString = type != TypeValue.Class ? type.ToString().ToLower() : fds.UserFunction.ReturnTypeValue;

        builder.AppendLine($"public static {typeString} {fds.Name} ({string.Join(", ", parameters)}) {{");
        
        IStatement body = fds.UserFunction.Body;
        if (body is not BlockStatement bs) builder.AppendLine(TranslateCodePart(body));
        else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateFunctionParameter((string name, string typeValue, TypeValue type) parameter)
    {
        string typeString = parameter.type != TypeValue.Class ? parameter.type.ToString().ToLower() : EscapeCSharpKeyword(parameter.typeValue);
        return $"{typeString} {EscapeCSharpKeyword(parameter.name)}";
    }

    private static string TranslateFunctionCallStatement(FunctionCallStatement fcs)
    {
        string[] args = new string[fcs.Args.Count];
        for (int i = 0; i < args.Length; i++) args[i] = TranslateExpression(fcs.Args[i]);

        return fcs.Name switch
        {
            "println" => $"RuntimeHelper.Println({string.Join(", ", args)});",
            "input" => $"RuntimeHelper.Input({string.Join(", ", args)});",
            "to_int" => $"RuntimeHelper.ToInt({string.Join(", ", args)});",
            "to_float" => $"RuntimeHelper.ToFloat({string.Join(", ", args)});",
            "to_double" => $"RuntimeHelper.ToDouble({string.Join(", ", args)});",
            "to_decimal" => $"RuntimeHelper.ToDecimal({string.Join(", ", args)});",
            "to_string" => $"RuntimeHelper.ToString({string.Join(", ", args)});",
            "to_boolean" => $"RuntimeHelper.ToBoolean({string.Join(", ", args)});",
            "len" => $"RuntimeHelper.GetLen({string.Join(", ", args)});",
            "type" => $"RuntimeHelper.GetType({string.Join(", ", args)});",
            _ => $"{fcs.Name}({string.Join(", ", args)});"
        };
    }

    private static string TranslateReturnStatement(ReturnStatement rs)
    {
        if (rs.Expression != null) return $"return {TranslateExpression(rs.Expression)};";
        return "return;";
    }

    private static string TranslateExpression(IExpression expression) => expression switch
    {
        LiteralExpression le => TranslateLiteralExpression(le),
        VariableExpression ve => ve.Name,
        ArrayExpression ae => TranslateArrayExpression(ae),
        ArrayFieldExpression afe => TranslateArrayFieldExpression(afe),
        UnaryExpression ue => TranslateUnaryExpression(ue),
        BinaryExpression be => TranslateBinaryExpression(be),
        TernaryExpression te => TranslateTernaryExpression(te),
        FunctionCallExpression fce => TranslateFunctionCallExpression(fce),
        NewClassExpression nce => TranslateNewClassExpression(nce),
        FieldExpression fe => TranslateFieldExpression(fe),
        MethodCallExpression mce => TranslateMethodCallExpression(mce),
        _ => throw new Exception($"Невозможно обработать данный тип выражения ({expression}).")
    };

    private static string TranslateLiteralExpression(LiteralExpression le)
    {
        string translatedExpression = le.Value.AsString();

        if (le.Value is StringValue) return $"\"{translatedExpression}\"";
        if (le.Value is BoolValue) return translatedExpression.ToLower();

        return $"{translatedExpression.Replace(",", ".")}{GetDefaultSuffixByType(le.Value.Type)}";
    }

    private static string TranslateArrayExpression(ArrayExpression ae) => $"{ae.Name}[{TranslateExpression(ae.Index)}]";

    private static string TranslateArrayFieldExpression(ArrayFieldExpression afe) => $"{TranslateExpression(afe.Target)}.{afe.Name}[{TranslateExpression(afe.Index)}]";

    private static string TranslateUnaryExpression(UnaryExpression ue) => $"{ue.Op.Value}{TranslateExpression(ue.Expression)}";

    private static string TranslateBinaryExpression(BinaryExpression be) => $"({TranslateExpression(be.Left)} {be.Op.Value} {TranslateExpression(be.Right)})";

    private static string TranslateTernaryExpression(TernaryExpression te) => $"({TranslateExpression(te.ConditionExpression)} ? {TranslateExpression(te.TrueExpression)} : {TranslateExpression(te.FalseExpression)})";

    private static string TranslateFunctionCallExpression(FunctionCallExpression fce)
    {
        string[] args = new string[fce.Args.Count];
        for (int i = 0; i < args.Length; i++) args[i] = TranslateExpression(fce.Args[i]);

        return fce.Name switch
        {
            "println" => $"RuntimeHelper.Println({string.Join(", ", args)})",
            "input" => $"RuntimeHelper.Input({string.Join(", ", args)})",
            "to_int" => $"RuntimeHelper.ToInt({string.Join(", ", args)})",
            "to_float" => $"RuntimeHelper.ToFloat({string.Join(", ", args)})",
            "to_double" => $"RuntimeHelper.ToDouble({string.Join(", ", args)})",
            "to_decimal" => $"RuntimeHelper.ToDecimal({string.Join(", ", args)})",
            "to_string" => $"RuntimeHelper.ToString({string.Join(", ", args)})",
            "to_boolean" => $"RuntimeHelper.ToBoolean({string.Join(", ", args)})",
            "len" => $"RuntimeHelper.GetLen({string.Join(", ", args)})",
            "type" => $"RuntimeHelper.GetType({string.Join(", ", args)})",
            _ => $"{fce.Name}({string.Join(", ", args)})"
        };
    }

    private static string TranslateNewClassExpression(NewClassExpression nce)
    {
        List<string> args = [];
        if (nce.Args != null) for (int i = 0; i < nce.Args.Count; i++) args.Add(TranslateExpression(nce.Args[i]));

        return $"new {nce.Name}({string.Join(", ", args)})";
    }

    private static string TranslateFieldExpression(FieldExpression fe)
    {
        string targetExpr = fe.Start != null && fe.Start is VariableExpression ve && ve.Name == "this" ? "" : $"{TranslateExpression(fe.Target)}.";
        return $"{targetExpr}{fe.Name}";
    }

    private static string TranslateMethodCallExpression(MethodCallExpression mce)
    {
        string[] args = new string[mce.Args.Count];
        for (int i = 0; i < args.Length; i++) args[i] = TranslateExpression(mce.Args[i]);
        string targetExpr = mce.Start != null && mce.Start is VariableExpression ve && ve.Name == "this" ? "" : $"{TranslateExpression(mce.Target)}.";
        return $"{targetExpr}{mce.MethodName}({string.Join(", ", args)})";
    }

    private static string GetDefaultSuffixByType(TypeValue type) => type switch
    {
        TypeValue.Int => "",
        TypeValue.Float => "f",
        TypeValue.Double => "d",
        TypeValue.Decimal => "m",
        _ => throw new Exception($"Невозможно определить суффикс нечислового типа '{type}'")
    };

    private static string GetDefaultValueByTypeValue(TypeValue type)
    {
        return type switch
        {
            TypeValue.Int => "0",
            TypeValue.Float => "0f",
            TypeValue.Double => "0d",
            TypeValue.Decimal => "0m",
            TypeValue.String => "\"\"",
            TypeValue.Bool => "false",
            _ => throw new Exception($"Невозможно указать стандартное значение типу {type}")
        };
    }

    private static string EscapeCSharpKeyword(string identifier)
    {
        // Список зарезервированных слов C#
        var keywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while", "file", "required", "init", "record", "with"
        };

        return keywords.Contains(identifier.ToLower()) ? $"@{identifier}" : identifier;
    }

    private static string GetFullTypeName(Type type)
    {
        if (type.Namespace == null) return type.Name;
        
        // Если тип находится в пространстве имен System, используем полный путь
        if (type.Namespace.StartsWith("System."))
        {
            return type.FullName ?? type.Name;
        }
        
        // Для пользовательских типов из библиотеки используем полный путь
        if (type.Assembly != typeof(object).Assembly)
        {
            return type.FullName ?? type.Name;
        }
        
        return type.Name;
    }
}
