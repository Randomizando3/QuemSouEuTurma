namespace QuemSouEuApp.Views;

public partial class CaptureClassPhotoPage : ContentPage
{
    public CaptureClassPhotoPage()
    {
        InitializeComponent();
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.CapturePhotoAsync();
        if (photo == null) return;

        // ? sempre salva em um novo arquivo seu (evita "capture(7).jpg" e locks estranhos)
        var newPath = Path.Combine(FileSystem.AppDataDirectory, $"class_{Guid.NewGuid():N}.jpg");

        await using (var inStream = await photo.OpenReadAsync())
        await using (var outStream = File.Open(newPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            await inStream.CopyToAsync(outStream);
        }

        // ? Mostra preview
        PhotoImg.Source = ImageSource.FromFile(newPath);

        // ? Vai pro fluxo multi-alunos
        await Shell.Current.GoToAsync($"classfacemark?path={Uri.EscapeDataString(newPath)}");
    }
}
