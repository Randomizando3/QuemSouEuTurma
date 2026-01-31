using System.Windows.Input;

namespace QuemSouEuApp.ViewModels;

public class HomeViewModel
{
    public ICommand CaptureClassCommand { get; }
    public ICommand StartGameCommand { get; }

    public HomeViewModel()
    {
        CaptureClassCommand = new Command(async () =>
            await Shell.Current.GoToAsync("capture"));

        StartGameCommand = new Command(async () =>
            await Shell.Current.GoToAsync("game"));
    }
}
