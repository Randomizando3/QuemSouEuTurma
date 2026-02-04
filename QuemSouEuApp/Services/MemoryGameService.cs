using QuemSouEuApp.Models;

namespace QuemSouEuApp.Services;

public static class MemoryGameService
{
    public static List<Student> PickRandomStudents(List<Student> all, int count)
    {
        if (count <= 0) return new List<Student>();
        if (all.Count <= count) return Shuffle(all);

        return Shuffle(all).Take(count).ToList();
    }

    public static List<MemoryCardTile> BuildDeck(List<Student> selectedStudents)
    {
        var deck = new List<MemoryCardTile>();

        foreach (var s in selectedStudents)
        {
            // PairKey pode ser o Id do aluno
            var key = s.Id;

            deck.Add(new MemoryCardTile { PairKey = key, FacePhotoPath = s.FacePhotoPath });
            deck.Add(new MemoryCardTile { PairKey = key, FacePhotoPath = s.FacePhotoPath });
        }

        return Shuffle(deck);
    }

    public static List<T> Shuffle<T>(IEnumerable<T> items)
    {
        var list = items.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}
