namespace Core.AST.Statements;

public class TryCatchFinallyStatement(List<IStatement> tryBlock, List<CatchBlock> catchBlocks, List<IStatement>? finallyBlock = null) : IStatement
{
    public List<IStatement> TryBlock { get; } = tryBlock;
    public List<CatchBlock> CatchBlocks { get; } = catchBlocks;
    public List<IStatement>? FinallyBlock { get; } = finallyBlock;
}

public class CatchBlock(string paramName, string paramType, List<IStatement> block)
{
    public string ParamName { get; } = paramName;
    public string ParamType { get; } = paramType;
    public List<IStatement> Block { get; } = block;
}
