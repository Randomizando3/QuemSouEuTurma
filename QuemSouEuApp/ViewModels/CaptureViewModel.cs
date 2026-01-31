using System.Windows.Input;

namespace QuemSouEuApp.ViewModels;

public class CaptureViewModel
{
    public string? PhotoPath { get; set; }

    public ICommand CaptureCommand { get; }

    public CaptureViewModel()
    {
        CaptureCommand = new Command(async () =>
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null) return;

            var newPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
            using var stream = await photo.OpenReadAsync();
            using var fs = File.OpenWrite(newPath);
            await stream.CopyToAsync(fs);

            PhotoPath = newPath;

            await Shell.Current.GoToAsync($"facemark?path={newPath}");
        });
    }
}
