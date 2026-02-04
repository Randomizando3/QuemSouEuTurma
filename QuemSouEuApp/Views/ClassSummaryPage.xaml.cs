using QuemSouEuApp.Services;

namespace QuemSouEuApp.Views;

[QueryProperty(nameof(ClassPhotoPath), "path")]
public partial class ClassSummaryPage : ContentPage
{
    public string ClassPhotoPath { get; set; } = "";

    public ClassSummaryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (!string.IsNullOrWhiteSpace(ClassPhotoPath) && File.Exists(ClassPhotoPath))
            ClassPhotoThumb.Source = ImageSource.FromFile(ClassPhotoPath);

        var all = await StorageService.LoadAsync();
        var list = all.Where(s => s.ClassPhotoPath == ClassPhotoPath).ToList();

        StudentsList.ItemsSource = list;
        CountLabel.Text = $"{list.Count} alunos";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (Shell.Current?.Navigation?.NavigationStack?.Count > 1)
            await Shell.Current.Navigation.PopAsync();
        else
            await Shell.Current.GoToAsync("///home");
    }

    private async void OnReplaceClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("capture");
    }

    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        // Tira uma foto avulsa (aluno novo) e depois marca o rosto igual
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Ops", "Câmera não disponível neste dispositivo.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return;

            // Salva local (mesma estratégia da turma)
            var dest = Path.Combine(FileSystem.AppDataDirectory, $"student_{Guid.NewGuid():N}.jpg");
            await using (var src = await photo.OpenReadAsync())
            await using (var dst = File.OpenWrite(dest))
                await src.CopyToAsync(dst);

            // Abre a página de marcação avulsa:
            // - photo: foto do aluno
            // - classPath: para amarrar na turma atual
            await Shell.Current.GoToAsync(
                $"singlefacemarking?photo={Uri.EscapeDataString(dest)}&classPath={Uri.EscapeDataString(ClassPhotoPath)}"
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", $"Falha ao capturar foto: {ex.Message}", "OK");
        }
    }
}
