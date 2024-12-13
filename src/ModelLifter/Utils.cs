
namespace ModelLifter;

public class ModelUtils {

    public static string? GetDirectoryInTreeThatContains(string currentDirectory, string targetDirectoryName)
    {
        bool found = false;
        foreach (string d in Directory.GetDirectories(currentDirectory, searchPattern: targetDirectoryName))
        {
            found = true;
            return Path.Combine(currentDirectory, targetDirectoryName);
        }
        if (!found)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(currentDirectory);
            if (dirInfo.Parent != null)
            {
                return GetDirectoryInTreeThatContains(Path.GetFullPath(Path.Combine(currentDirectory, "..")), targetDirectoryName);
            }
            else
            {
                return null;
            }
        }
        return null;
    }

}