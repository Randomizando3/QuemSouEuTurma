namespace QuemSouEuApp.Services;

public static class ClassStateService
{
    private static string CurrentPathFile =>
        Path.Combine(FileSystem.AppDataDirectory, "class_current.txt");

    public static async Task SetCurrentClassPhotoPathAsync(string classPhotoPath)
    {
        await File.WriteAllTextAsync(CurrentPathFile, classPhotoPath ?? "");
    }

    public static async Task<string?> GetCurrentClassPhotoPathAsync()
    {
        if (!File.Exists(CurrentPathFile))
            return null;

        var path = (await File.ReadAllTextAsync(CurrentPathFile)).Trim();
        if (string.IsNullOrWhiteSpace(path))
            return null;

        return path;
    }

    public static async Task<bool> HasCurrentClassAsync()
    {
        var path = await GetCurrentClassPhotoPathAsync();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;

        // precisa ter ao menos 1 aluno para considerar "turma cadastrada"
        var students = await StorageService.LoadAsync();
        return students.Any(s => s.ClassPhotoPath == path);
    }

    public static Task ClearCurrentClassAsync()
    {
        if (File.Exists(CurrentPathFile))
            File.Delete(CurrentPathFile);

        return Task.CompletedTask;
    }
}
