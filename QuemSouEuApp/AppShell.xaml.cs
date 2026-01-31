using QuemSouEuApp.Views;

namespace QuemSouEuApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("capture", typeof(CaptureClassPhotoPage));
        Routing.RegisterRoute("game", typeof(GamePage));
        Routing.RegisterRoute("result", typeof(ResultPage));
        Routing.RegisterRoute("classfacemark", typeof(ClassFaceMarkingPage));
        Routing.RegisterRoute("classsummary", typeof(ClassSummaryPage));
    }
}
