using QuemSouEuApp.Models;
using QuemSouEuApp.Services;
using Microsoft.Maui.Graphics;

namespace QuemSouEuApp.ViewModels;

public class FaceMarkingViewModel : IDrawable
{
    public string StudentName { get; set; } = "";
    private Rect _faceRect;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 3;
        canvas.DrawRectangle(_faceRect);
    }

    public Command SaveCommand => new(async () =>
    {
        var students = await StorageService.LoadAsync();
        students.Add(new Student
        {
            Name = StudentName,
            Face = new FaceRegion
            {
                X = (float)_faceRect.X,
                Y = (float)_faceRect.Y,
                Width = (float)_faceRect.Width,
                Height = (float)_faceRect.Height
            }
        });

        await StorageService.SaveAsync(students);
        await Shell.Current.GoToAsync("..");
    });
}
