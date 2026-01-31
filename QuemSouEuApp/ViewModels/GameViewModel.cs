using QuemSouEuApp.Services;

namespace QuemSouEuApp.ViewModels;

public class GameViewModel
{
    public string Guess { get; set; } = "";
    private string _secretName = "";

    public Command GuessCommand => new(() =>
    {
        if (Guess.Equals(_secretName, StringComparison.OrdinalIgnoreCase))
            Shell.Current.GoToAsync("result?win=true");
        else
            Shell.Current.GoToAsync("result?win=false");
    });
}
