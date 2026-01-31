using System.Text.Json;
using QuemSouEuApp.Models;

namespace QuemSouEuApp.Services;

public static class StorageService
{
    private static string BasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "students.json");

    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true
    };

    public static async Task SaveAsync(List<Student> students)
    {
        var json = JsonSerializer.Serialize(students, _opts);
        await File.WriteAllTextAsync(BasePath, json);
    }

    public static async Task<List<Student>> LoadAsync()
    {
        if (!File.Exists(BasePath))
            return new();

        var json = await File.ReadAllTextAsync(BasePath);
        return JsonSerializer.Deserialize<List<Student>>(json, _opts) ?? new();
    }
}
