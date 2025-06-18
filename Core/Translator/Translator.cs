using Core.AST.Statements;
using Core.Expressions;
using Core.Values;
using System.Text;
using System.Reflection;

namespace Core.Translator;

/// <summary>
/// Class to translate Coddy code to C# code
/// </summary>
public class Translator
{
    private static ImportManager? importManager;

    public static string Translate(List<IStatement> statements)
    {
        importManager = new ImportManager(new Translator());
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

        foreach (IStatement statement in statementsInGlobalClass)
        {
            if (statement is IncludeStatement includeStatement)
            {
                string importCode = importManager.ProcessImport(includeStatement);
                if (!string.IsNullOrEmpty(importCode))
                {
                    builder.AppendLine(importCode);
                }
            }
        }

        builder.AppendLine("public class __Program__ {");
        builder.AppendLine("public static void __Main__() {");
        foreach (IStatement statement in statementsInMain) builder.AppendLine($"{TranslateCodePart(statement)}");
        builder.AppendLine("}");

        foreach (IStatement statement in statementsInGlobalClass) 
        {
            if (statement is not IncludeStatement) builder.AppendLine($"{TranslateCodePart(statement)}");
        }

        builder.AppendLine("}");
        
        importManager.Clear();
        return builder.ToString();
    }

    private static bool CodePartInGlobal(IStatement statement) => statement switch
    {
        IncludeStatement or UseStatement or ClassDeclarationStatement or FunctionDeclarationStatement or EnumDeclarationStatement or InterfaceDeclarationStatement => true,
        _ => false
    };

    private static string TranslateCodePart(IStatement statement) => statement switch
    {
        UseStatement us => TranslateUseStatement(us),
        ClassDeclarationStatement cds => TranslateClassDeclarationStatement(cds),
        FieldDeclarationStatement fds => TranslateFieldDeclarationStatement(fds),
        FieldArrayDeclarationStatement fads => TranslateFieldArrayDeclarationStatement(fads),
        FieldAssignmentStatement fas => TranslateFieldAssignmentStatement(fas),
        MethodDeclarationStatement mds => TranslateMethodDeclarationStatement(mds),
        MethodCallStatement mcs => TranslateMethodCallStatement(mcs),
        ClassEnumDeclarationStatement ceds => TranslateClassEnumDeclarationStatement(ceds),
        ClassInterfaceDeclarationStatement cids => TranslateClassInterfaceDeclarationStatement(cids),
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
        EnumDeclarationStatement eds => TranslateEnumDeclarationStatement(eds),
        InterfaceDeclarationStatement ids => TranslateInterfaceDeclarationStatement(ids),
        ReturnStatement rs => TranslateReturnStatement(rs),
        BreakStatement => "break;",
        ContinueStatement => "continue;",
        ThrowStatement ts => $"throw {TranslateExpression(ts.Expression)};",
        TryCatchFinallyStatement tcfs => TranslateTryCatchFinallyStatement(tcfs),
        LambdaExpressionStatement les => TranslateLambdaExpressionStatement(les),
        _ => throw new Exception($"Невозможно обработать данный тип команды ({statement}).")
    };

    public string TranslateIncludeStatement(IncludeStatement ins)
    {
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries", TranslateExpression(ins.LibraryPathExpression).TrimEnd('\"').TrimStart('\"'));

        if (!File.Exists(fullPath)) throw new Exception($"Библиотека по пути {fullPath} не найдена.");

        string extension = Path.GetExtension(fullPath).ToLower();
        return extension switch
        {
            ".cd" => TranslateCoddyLibrary(fullPath),
            ".dll" => TranslateDLLLibrary(fullPath),
            ".cs" => TranslateCSharpLibrary(fullPath),
            _ => throw new Exception($"Файл с расширением '{extension}' не является поддерживаемым библиотечным файлом. Поддерживаемые расширения: .cd, .dll, .cs."),
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
            if (statement is IncludeStatement includeStatement)
            {
                string importCode = importManager?.ProcessImport(includeStatement) ?? string.Empty;
                if (!string.IsNullOrEmpty(importCode)) builder.AppendLine(importCode);
            }
            else if (statement is ClassDeclarationStatement classDeclaration) builder.AppendLine(TranslateClassDeclarationStatement(classDeclaration));
            else if (statement is EnumDeclarationStatement enumDeclaration) builder.AppendLine(TranslateEnumDeclarationStatement(enumDeclaration));
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
        catch (Exception ex) { throw new Exception($"Ошибка при трансляции C# библиотеки: {ex.Message}"); }
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

    private static string TranslateUseStatement(UseStatement us)
    {
        string filePath = TranslateExpression(us.FilePathExpression).TrimEnd('\"').TrimStart('\"');

        if (!File.Exists(filePath)) throw new Exception($"Файл по пути {filePath} не существует.");

        if (Path.GetExtension(filePath).ToLower() != ".cd") throw new Exception($"Расширение {Path.GetExtension(filePath).ToLower()} файла по пути {filePath} не поддерживается.");

        return TranslateCoddyLibrary(filePath);
    }

    private static string TranslateClassDeclarationStatement(ClassDeclarationStatement cds)
    {
        StringBuilder builder = new();
        bool isStatic = cds.ClassInfo.IsStatic;
        string isStaticString = isStatic ? "static" : "";
        string className = EscapeCSharpKeyword(cds.ClassInfo.Name);
        cds.ClassInfo.Implements = [.. cds.ClassInfo.Implements.Select(i => EscapeCSharpKeyword(i))];
        string genericsParameters = "";
        if (cds.ClassInfo.GenericsParameters != null) genericsParameters = $"<{cds.ClassInfo.GenericsParameters}> ";

        builder.Append($"public {isStaticString} class {className} {genericsParameters}");
        if (cds.ClassInfo.Implements.Count > 0) builder.Append($": {string.Join(", ", cds.ClassInfo.Implements)} ");
        builder.AppendLine("{");
        foreach (IStatement statement in cds.Statements) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateFieldDeclarationStatement(FieldDeclarationStatement fds)
    {
        StringBuilder builder = new();

        string fieldName = EscapeCSharpKeyword(fds.Name);
        builder.Append($"{fds.Access.ToString().ToLower()} {string.Join(" ", fds.Modifiers)} {string.Join(".", fds.TypeExpressions)} {fieldName} ");
        if (fds.HasGetter || fds.HasSetter)
        {
            builder.Append("{ ");
            if (fds.HasGetter) builder.Append("get; ");
            if (fds.HasSetter) builder.Append("set; ");
            builder.Append("} ");
        }
        if (fds.Expression != null) builder.Append($"= {TranslateExpression(fds.Expression)}");
        if (!fds.HasGetter && !fds.HasSetter) builder.Append(';');

        return builder.ToString();
    }

    private static string TranslateFieldArrayDeclarationStatement(FieldArrayDeclarationStatement fads)
    {
        StringBuilder builder = new();

        string sizeString = fads.Size != null ? TranslateExpression(fads.Size) : "";

        string expression = "";
        if (fads.Expression != null)
        {
            if (fads.Expression is ArrayDeclarationExpression arrayDeclarationExpression) expression = TranslateArrayDeclarationExpression(arrayDeclarationExpression, string.Join(".", fads.TypeExpressions), sizeString);
            else expression = TranslateExpression(fads.Expression);
        }
        else
        {
            StringBuilder initBuilder = new();
            initBuilder.Append($"new {string.Join(".", fads.TypeExpressions)}[{sizeString}] {{ ");
            int size = fads.Size != null ? int.Parse(TranslateExpression(fads.Size)) : 0;
            string[] expressions = new string[size];
            for (int i = 0; i < expressions.Length; i++) expressions[i] = GetDefaultValueByStringTypeValue(fads.TypeExpressions[^1]);

            initBuilder.Append(string.Join(", ", expressions));
            initBuilder.Append(" }");

            expression = initBuilder.ToString();
        }

        builder.Append($"{fads.Access.ToString().ToLower()} {string.Join(" ", fads.Modifiers)} {string.Join(".", fads.TypeExpressions)}[] {fads.Name} ");

        if (fads.HasGetter || fads.HasSetter)
        {
            builder.Append("{ ");
            if (fads.HasGetter) builder.Append("get; ");
            if (fads.HasSetter) builder.Append("set; ");
            builder.Append("} ");
        }

        builder.Append($"= {expression};");

        return builder.ToString();
    }

    private static string TranslateFieldAssignmentStatement(FieldAssignmentStatement fas)
    {
        string thisContext = $"{TranslateExpression(fas.TargetExpression)}.";
        string expression = TranslateExpression(fas.Expression);
        if (fas.Expression is ArrayDeclarationExpression ads) expression = TranslateArrayDeclarationExpression(ads, ads.TypeExpression);

        return $"{thisContext}{fas.Name} {fas.OpToken.Value} {expression};";
    }

    private static string TranslateMethodDeclarationStatement(MethodDeclarationStatement mds)
    {
        StringBuilder builder = new();
        string[] parameters = new string[mds.Method.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter(mds.Method.Parameters[i]);

        string methodName = EscapeCSharpKeyword(mds.MethodName);

        string genericsParameters = "";
        if (mds.Method.GenericsParameters != null) genericsParameters = $"<{mds.Method.GenericsParameters}>";
        IStatement body = mds.Method.Body;

        builder.Append($"{mds.Access.ToString().ToLower()} {string.Join(" ", mds.Modifiers)} {mds.Method.ReturnType} {methodName}{genericsParameters} ({string.Join(", ", parameters)})");
        if (body is not LambdaExpressionStatement)
        {
            builder.AppendLine(" {");
            if (body is not BlockStatement bs) builder.AppendLine(TranslateCodePart(body));
            else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
            builder.AppendLine("}");
        }
        else builder.AppendLine(TranslateCodePart(body));

        return builder.ToString();
    }

    private static string TranslateMethodCallStatement(MethodCallStatement mcs) => $"{TranslateExpression(mcs.Expression)};";

    private static string TranslateClassEnumDeclarationStatement(ClassEnumDeclarationStatement ceds)
    {
        StringBuilder builder = new();
        string[] members = new string[ceds.Members.Count];
        for (int i = 0; i < members.Length; i++)
        {
            string memberName = ceds.Members[i].Name;
            string value = "";
            if (ceds.Members[i].Expression != null) value = $" = {TranslateExpression(ceds.Members[i].Expression!)}";
            members[i] = $"{memberName}{value}";
        }

        builder.AppendLine($"{ceds.Access.ToString().ToLower()} enum {ceds.Name} {{");
        builder.AppendLine(string.Join(", ", members));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateClassInterfaceDeclarationStatement(ClassInterfaceDeclarationStatement cids)
    {
        StringBuilder builder = new();
        cids.Implements = [.. cids.Implements.Select(i => EscapeCSharpKeyword(i))];
        builder.AppendLine($"{cids.Access.ToString().ToLower()} interface {cids.Name} ");
        if (cids.Implements.Count > 0) builder.Append($": {string.Join(", ", cids.Implements)}");
        builder.AppendLine("{");

        foreach ((string, string, List<(string, string)>) method in cids.Methods)
        {
            string[] parameters = new string[method.Item3.Count];
            for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter((method.Item3[i].Item1, method.Item3[i].Item2));
            builder.AppendLine($"{method.Item2} {method.Item1}({string.Join(", ", parameters)});");
        }
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateConstructorDeclarationStatement(ConstructorDeclarationStatement cds)
    {
        StringBuilder builder = new();
        string[] parameters = new string[cds.Constructor.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter(cds.Constructor.Parameters[i]);
        IStatement body = cds.Constructor.Body;

        builder.Append($"public {cds.ClassInfo.Name}({string.Join(", ", parameters)})");
        if (cds.ParentParameters.Count != 0)
        {
            string[] parentParameters = new string[cds.ParentParameters.Count];
            for (int i = 0; i < parentParameters.Length; i++) parentParameters[i] = TranslateExpression(cds.ParentParameters[i]);
            builder.AppendLine($" : base({string.Join(", ", parentParameters)})");
        }
        if (body is not LambdaExpressionStatement)
        {
            builder.AppendLine(" {");
            if (body is not BlockStatement bs) builder.AppendLine(TranslateCodePart(body));
            else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
            builder.AppendLine("}");
        }
        else builder.AppendLine(TranslateCodePart(body));

        return builder.ToString();
    }

    private static string TranslateVariableDeclarationStatement(VariableDeclarationStatement vds)
    {
        StringBuilder builder = new();
        
        builder.Append($"{string.Join(".", vds.TypeExpressions)} {vds.Name} = ");
        if (vds.Expression != null) builder.Append($"{TranslateExpression(vds.Expression)}");
        else builder.Append(GetDefaultValueByStringTypeValue(vds.TypeExpressions[^1]));
        builder.Append(';');
        
        return builder.ToString();
    }

    private static string TranslateArrayDeclarationStatement(ArrayDeclarationStatement ads)
    {
        StringBuilder builder = new();

        string sizeString = ads.Size != null ? TranslateExpression(ads.Size) : "";

        string expression = "";
        if (ads.Expression != null)
        {
            if (ads.Expression is ArrayDeclarationExpression arrayDeclarationExpression) expression = TranslateArrayDeclarationExpression(arrayDeclarationExpression, string.Join(".", ads.TypeExpressions), sizeString);
            else expression = TranslateExpression(ads.Expression);
        }
        else
        {
            StringBuilder initBuilder = new();
            initBuilder.Append($"new {string.Join(".", ads.TypeExpressions)}[{sizeString}]");
            /*ads.TypeExpressions = [.. ads.TypeExpressions.Select(type => GetDefaultValueByStringTypeValue(type))];

            initBuilder.Append(string.Join(".", ads.TypeExpressions));*/
            //initBuilder.Append(" }");

            expression = initBuilder.ToString();
        }

        builder.Append($"{string.Join(".", ads.TypeExpressions)}[] {ads.Name} = {expression};");

        return builder.ToString();
    }

    private static string TranslateArrayAssignmentStatement(ArrayAssignmentStatement aas) => $"{aas.Name}[{TranslateExpression(aas.Index)}] = {TranslateExpression(aas.Expression)};";

    private static string TranslateAssignmentStatement(AssignmentStatement ags)
    {
        string expression = TranslateExpression(ags.NewExpression);
        if (ags.NewExpression is ArrayDeclarationExpression ade) expression = TranslateArrayDeclarationExpression(ade, ade.TypeExpression);
        return $"{ags.Name} = {expression};";
    }

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
        builder.AppendLine($"for ({TranslateCodePart(fls.IndexerDeclaration)} {TranslateExpression(fls.Condition)}; {TranslateCodePart(fls.Iterator).TrimEnd(';')}) {{");
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

        string genericsParameters = "";
        if (fds.UserFunction.GenericsParameters != null) genericsParameters = $"<{fds.UserFunction.GenericsParameters}>";
        IStatement body = fds.UserFunction.Body;

        builder.AppendLine($"public static {fds.UserFunction.ReturnType} {fds.Name}{genericsParameters} ({string.Join(", ", parameters)})");
        if (body is not LambdaExpressionStatement)
        {
            builder.AppendLine(" {");
            if (body is not BlockStatement bs) builder.AppendLine(TranslateCodePart(body));
            else foreach (IStatement statement in bs.Statements) builder.AppendLine(TranslateCodePart(statement));
        
            builder.AppendLine("}");
        }
        else builder.AppendLine(TranslateCodePart(body));

        return builder.ToString();
    }

    private static string TranslateFunctionParameter((string name, string typeExpression) parameter) => $"{parameter.typeExpression} {parameter.name}";

    private static string TranslateFunctionCallStatement(FunctionCallStatement fcs)
    {
        string[] args = new string[fcs.Args.Count];
        for (int i = 0; i < args.Length; i++) args[i] = TranslateExpression(fcs.Args[i]);

        return fcs.Name switch
        {
            "print" => $"RuntimeHelper.Print({string.Join(", ", args)});",
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

    private static string TranslateEnumDeclarationStatement(EnumDeclarationStatement eds)
    {
        string name = eds.Name;
        string[] members = new string[eds.Members.Count];
        for (int i = 0; i < members.Length; i++)
        {
            string memberName = eds.Members[i].Name;
            string value = "";
            if (eds.Members[i].Expression != null) value = $" = {TranslateExpression(eds.Members[i].Expression!)}";
            members[i] = $"{memberName}{value}";
        }

        StringBuilder builder = new();
        builder.AppendLine($"public enum {name} {{");
        builder.AppendLine(string.Join(", ", members));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateInterfaceDeclarationStatement(InterfaceDeclarationStatement ids)
    {
        StringBuilder builder = new();
        ids.Implements = [.. ids.Implements.Select(i => EscapeCSharpKeyword(i))];
        builder.Append($"public interface {ids.Name} ");
        if (ids.Implements.Count > 0) builder.Append($": {string.Join(", ", ids.Implements)}");
        builder.AppendLine("{");

        foreach ((string, string, List<(string, string)>) method in ids.Methods)
        {
            string[] parameters = new string[method.Item3.Count];
            for (int i = 0; i < parameters.Length; i++) parameters[i] = TranslateFunctionParameter((method.Item3[i].Item1, method.Item3[i].Item2));
            builder.AppendLine($"{method.Item2} {method.Item1}({string.Join(", ", parameters)});");
        }
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string TranslateReturnStatement(ReturnStatement rs)
    {
        if (rs.Expression != null) return $"return {TranslateExpression(rs.Expression)};";
        return "return;";
    }

    private static string TranslateTryCatchFinallyStatement(TryCatchFinallyStatement tcfs)
    {
        StringBuilder builder = new();

        builder.AppendLine("try {");
        foreach (IStatement statement in tcfs.TryBlock) builder.AppendLine(TranslateCodePart(statement));
        builder.AppendLine("}");

        foreach (CatchBlock catchBlock in tcfs.CatchBlocks)
        {
            builder.AppendLine($"catch ({catchBlock.ParamType} {catchBlock.ParamName}) {{");
            foreach (IStatement statement in catchBlock.Block) builder.AppendLine(TranslateCodePart(statement));
            builder.AppendLine("}");
        }

        if (tcfs.FinallyBlock != null)
        {
            builder.AppendLine("finally {");
            foreach (IStatement statement in tcfs.FinallyBlock) builder.AppendLine(TranslateCodePart(statement));
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static string TranslateLambdaExpressionStatement(LambdaExpressionStatement les)
    {
        if (les.Parameters == null) return $" => {TranslateExpression(les.Expression)};";

        string[] parameters = new string[les.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = $"{les.Parameters[i].Item2} {les.Parameters[i].Item1}";

        return $"({string.Join(", ", parameters)}) => {TranslateExpression(les.Expression)};";
    }

    public static string TranslateExpression(IExpression expression) => expression switch
    {
        LiteralExpression le => TranslateLiteralExpression(le),
        VariableExpression ve => ve.Name,
        ArrayExpression ae => TranslateArrayExpression(ae),
        ArrayFieldExpression afe => TranslateArrayFieldExpression(afe),
        ArrayDeclarationExpression ade => TranslateArrayDeclarationExpression(ade),
        AssignmentExpression ae => TranslateAssignmentExpression(ae),
        UnaryExpression ue => TranslateUnaryExpression(ue),
        BinaryExpression be => TranslateBinaryExpression(be),
        TernaryExpression te => TranslateTernaryExpression(te),
        FunctionCallExpression fce => TranslateFunctionCallExpression(fce),
        NewClassExpression nce => TranslateNewClassExpression(nce),
        FieldExpression fe => TranslateFieldExpression(fe),
        MethodCallExpression mce => TranslateMethodCallExpression(mce),
        LambdaExpression le => TranslateLambdaExpression(le),
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

    private static string TranslateArrayDeclarationExpression(ArrayDeclarationExpression ade, string? typeString = null, string? sizeString = null)
    {
        string size = sizeString ?? "";

        string[] elements = new string[ade.Expressions.Count];
        for (int i = 0; i < elements.Length; i++) elements[i] = TranslateExpression(ade.Expressions[i]);

        return $"new {typeString!}[{size}] {{ {string.Join(", ", elements)} }}";
    }

    private static string TranslateAssignmentExpression(AssignmentExpression ae)
    {
        string thisContext = $"{TranslateExpression(ae.TargetExpression)}.";
        string expression = TranslateExpression(ae.Expression);
        if (ae.Expression is ArrayDeclarationExpression ads) expression = TranslateArrayDeclarationExpression(ads, ads.TypeExpression);

        return $"{thisContext}{ae.Name} = {expression}";
    }

    private static string TranslateUnaryExpression(UnaryExpression ue) => $"{ue.Op.Value}{TranslateExpression(ue.Expression)}";

    private static string TranslateBinaryExpression(BinaryExpression be) => $"({TranslateExpression(be.Left)} {be.Op.Value} {TranslateExpression(be.Right)})";

    private static string TranslateTernaryExpression(TernaryExpression te) => $"({TranslateExpression(te.ConditionExpression)} ? {TranslateExpression(te.TrueExpression)} : {TranslateExpression(te.FalseExpression)})";

    private static string TranslateFunctionCallExpression(FunctionCallExpression fce)
    {
        string[] args = new string[fce.Args.Count];
        for (int i = 0; i < args.Length; i++) args[i] = TranslateExpression(fce.Args[i]);

        return fce.Name switch
        {
            "print" => $"RuntimeHelper.Print({string.Join(", ", args)})",
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
        string genericsParameters = "";
        if (nce.GenericsParameters != null) genericsParameters = $"<{nce.GenericsParameters}>";

        List<string> args = [];
        if (nce.Args != null) for (int i = 0; i < nce.Args.Count; i++) args.Add(TranslateExpression(nce.Args[i]));

        return $"new {nce.Name}{genericsParameters}({string.Join(", ", args)})";
    }

    private static string TranslateFieldExpression(FieldExpression fe) => $"{TranslateExpression(fe.Target)}.{fe.Name}";

    private static string TranslateMethodCallExpression(MethodCallExpression mce)
    {
        string[] args = new string[mce.Args.Count];
        for (int i = 0; i < args.Length; i++) args[i] = TranslateExpression(mce.Args[i]);
        string targetExpr = $"{TranslateExpression(mce.Target)}.";
        return $"{targetExpr}{mce.MethodName}({string.Join(", ", args)})";
    }

    private static string TranslateLambdaExpression(LambdaExpression le)
    {
        string[] parameters = new string[le.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++) parameters[i] = $"{le.Parameters[i].Item2} {le.Parameters[i].Item1}";

        return $"({string.Join(", ", parameters)}) => {TranslateExpression(le.Expression)}";
    }

    private static string GetDefaultSuffixByType(TypeValue type) => type switch
    {
        TypeValue.Int => "",
        TypeValue.Float => "f",
        TypeValue.Double => "d",
        TypeValue.Decimal => "m",
        _ => throw new Exception($"Невозможно определить суффикс нечислового типа '{type}'")
    };

    private static string GetDefaultValueByStringTypeValue(string type)
    {
        return type switch
        {
            "int" => "0",
            "float" => "0f",
            "double" => "0d",
            "decimal" => "0m",
            "string" => "\"\"",
            "bool" => "false",
            _ => type/*throw new Exception($"Невозможно указать стандартное значение типу {type}")*/
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
