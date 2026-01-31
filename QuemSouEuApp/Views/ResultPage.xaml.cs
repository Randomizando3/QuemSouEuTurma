namespace QuemSouEuApp.Views;

[QueryProperty(nameof(Win), "win")]
public partial class ResultPage : ContentPage
{
    public string Win { get; set; } = "0";

    public ResultPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Msg.Text = (Win == "1")
            ? "?? Parabéns!!!"
            : "?? Quase! Tenta de novo!";
    }
}
