namespace QuemSouEuApp.Models;

public sealed class GameStudentTile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FacePhotoPath { get; set; } = "";

    public bool IsHidden { get; set; }
}
