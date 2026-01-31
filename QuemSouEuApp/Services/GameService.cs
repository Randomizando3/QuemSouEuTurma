using QuemSouEuApp.Models;

namespace QuemSouEuApp.Services;

public static class GameService
{
    private static readonly Random _rng = new();

    public static async Task<(string classPhotoPath, List<Student> students)> LoadCurrentClassAsync()
    {
        var path = await ClassStateService.GetCurrentClassPhotoPathAsync();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return ("", new List<Student>());

        var all = await StorageService.LoadAsync();
        var list = all.Where(s => s.ClassPhotoPath == path && !string.IsNullOrWhiteSpace(s.FacePhotoPath)).ToList();

        return (path, list);
    }

    public static Student? PickSecret(List<Student> students)
    {
        if (students == null || students.Count == 0) return null;
        return students[_rng.Next(students.Count)];
    }

    public static List<Student> Shuffle(List<Student> students)
    {
        var copy = students.ToList();
        for (int i = copy.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }
}
