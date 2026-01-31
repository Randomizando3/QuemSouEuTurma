namespace QuemSouEuApp.Models;

public sealed class Student
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";

    // caminho da foto geral da turma usada para marcar
    public string ClassPhotoPath { get; set; } = "";

    // caminho da foto recortada do rosto (gerada)
    public string FacePhotoPath { get; set; } = "";

    // retângulo marcado em cima do overlay (por enquanto)
    public FaceRegion? Face { get; set; }
}
