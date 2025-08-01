﻿using Core.Expressions;

namespace Core.AST.Statements;

public class InterfaceDeclarationStatement(string name, List<string> implements, List<(string, string, bool, bool, bool, IExpression?)> fields, List<(string, string, List<(string, string)>)> methods) : IStatement
{
    public string Name { get; } = name;
    public List<string> Implements { get; set; } = implements;
    public List<(string, string, bool, bool, bool, IExpression?)> Fields { get; } = fields;
    public List<(string, string, List<(string, string)>)> Methods { get; } = methods;
}
