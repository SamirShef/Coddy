using Core.AST.Statements;

namespace Core.Translator;

public class ImportManager(Translator translator)
{
    private readonly HashSet<string> importedLibraries = [];
    private readonly Translator translator = translator;

    public string ProcessImport(IncludeStatement includeStatement)
    {
        string libraryPath = includeStatement.LibraryPath;
        
        if (importedLibraries.Contains(libraryPath)) return string.Empty;

        importedLibraries.Add(libraryPath);
        return translator.TranslateIncludeStatement(includeStatement);
    }

    public void Clear() => importedLibraries.Clear();
}
