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

        if (!string.IsNullOrWhiteSpace(ClassPhotoPath) && File.Exists(ClassPhotoPath))
            ClassPhotoThumb.Source = ImageSource.FromFile(ClassPhotoPath);

        var all = await StorageService.LoadAsync();
        var list = all.Where(s => s.ClassPhotoPath == ClassPhotoPath).ToList();
        StudentsList.ItemsSource = list;
    }

    private async void OnReplaceClicked(object sender, EventArgs e)
    {
        // Apenas inicia novo fluxo de captura.
        // Se você quiser apagar a turma anterior, eu coloco isso também.
        await Shell.Current.GoToAsync("capture");
    }
}
