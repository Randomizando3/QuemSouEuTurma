using QuemSouEuApp.Services;

namespace QuemSouEuApp.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnClassesTapped(object sender, EventArgs e)
    {
        // Se já existe turma atual, abre o resumo
        if (await ClassStateService.HasCurrentClassAsync())
        {
            var path = await ClassStateService.GetCurrentClassPhotoPathAsync();
            await Shell.Current.GoToAsync($"classsummary?path={Uri.EscapeDataString(path!)}");
            return;
        }

        // Senão, inicia fluxo de cadastro
        await Shell.Current.GoToAsync("capture");
    }

    private async void OnWhoAmITapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("game");
    }

    private async void OnMemoryTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("memory");
    }
}
