using System.Text.Json;

namespace HHSEditor.Core.Services;

/// <summary>
/// JSON 数据服务实现。
/// </summary>
public sealed class JsonDataService : IDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> LoadAsync<T>(string filePath, CancellationToken ct = default) where T : class
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveAsync<T>(string filePath, T data, CancellationToken ct = default) where T : class
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, data, JsonOptions, ct);
    }

    public async Task<IReadOnlyList<T>> LoadDirectoryAsync<T>(string directoryPath, string searchPattern = "*.json", CancellationToken ct = default) where T : class
    {
        if (!Directory.Exists(directoryPath))
        {
            return [];
        }

        List<T> results = [];
        foreach (string file in Directory.GetFiles(directoryPath, searchPattern))
        {
            T? item = await LoadAsync<T>(file, ct);
            if (item != null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public IReadOnlyList<string> ListFiles(string directoryPath, string searchPattern = "*.json")
    {
        if (!Directory.Exists(directoryPath))
        {
            return [];
        }

        return Directory.GetFiles(directoryPath, searchPattern);
    }
}
