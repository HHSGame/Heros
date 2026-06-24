using System.Text.Json;
using System.Text.Json.Serialization;

namespace HHSEditor.Core.Services;

/// <summary>
/// JSON 数据服务实现。
/// </summary>
public sealed class JsonDataService : IDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<T?> LoadAsync<T>(string filePath, CancellationToken ct = default) where T : class
    {
        Console.WriteLine($"[JsonDataService] LoadAsync called for: {filePath}");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[JsonDataService] File not found: {filePath}");
            return null;
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath, ct);
            Console.WriteLine($"[JsonDataService] File content length: {json.Length}");

            using FileStream stream = File.OpenRead(filePath);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
            Console.WriteLine($"[JsonDataService] Deserialized result: {result?.GetType().Name ?? "null"}");
            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[JsonDataService] JsonException: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JsonDataService] Exception: {ex.Message}");
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
