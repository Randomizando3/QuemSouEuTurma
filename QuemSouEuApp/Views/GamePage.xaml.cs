using System.Collections.ObjectModel;
using QuemSouEuApp.Models;
using QuemSouEuApp.Services;

namespace QuemSouEuApp.Views;

public partial class GamePage : ContentPage
{
    private readonly ObservableCollection<GameStudentTile> _tiles = new();

    private Student? _secret;
    private string _classPhotoPath = "";

    public GamePage()
    {
        InitializeComponent();
        FacesGrid.ItemsSource = _tiles;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadClassAndStartAsync();
    }

    private async Task LoadClassAndStartAsync()
    {
        FeedbackLabel.Text = "";
        GuessEntry.Text = "";

        var (classPath, students) = await GameService.LoadCurrentClassAsync();
        _classPhotoPath = classPath;

        if (string.IsNullOrWhiteSpace(_classPhotoPath) || students.Count == 0)
        {
            StatusLabel.Text = "Nenhuma turma cadastrada.";
            HintLabel.Text = "Vá em “Foto da Turma” e cadastre a turma primeiro.";
            _tiles.Clear();
            _secret = null;
            return;
        }

        StatusLabel.Text = $"Turma carregada: {students.Count} alunos";
        HintLabel.Text = "Toque nos rostos para ocultar. Depois digite o nome e confirme.";

        StartNewRound(students);
    }

    private void StartNewRound(List<Student> students)
    {
        FeedbackLabel.Text = "";
        GuessEntry.Text = "";

        var shuffled = GameService.Shuffle(students);
        _secret = GameService.PickSecret(shuffled);

        _tiles.Clear();
        foreach (var s in shuffled)
        {
            _tiles.Add(new GameStudentTile
            {
                Id = s.Id,
                Name = s.Name,
                FacePhotoPath = s.FacePhotoPath,
                IsHidden = false
            });
        }
    }

    private async void OnCopySecretClicked(object sender, EventArgs e)
    {
        if (_secret == null)
        {
            await DisplayAlert("Ops", "Nenhum aluno secreto sorteado.", "OK");
            return;
        }

        await Clipboard.Default.SetTextAsync(_secret.Name);

        FeedbackLabel.Text = "? Nome copiado para a área de transferência.";
    }

    private async void OnNewSecretClicked(object sender, EventArgs e)
    {
        var (_, students) = await GameService.LoadCurrentClassAsync();
        if (students.Count == 0)
        {
            await DisplayAlert("Ops", "Cadastre uma turma primeiro.", "OK");
            return;
        }

        StartNewRound(students);
        FeedbackLabel.Text = "?? Novo aluno secreto sorteado.";
    }

    private void OnTileTapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement ve) return;
        if (ve.BindingContext is not GameStudentTile tile) return;

        tile.IsHidden = !tile.IsHidden;

        // força refresh simples (ObservableCollection não notifica mudança interna)
        var idx = _tiles.IndexOf(tile);
        if (idx >= 0)
        {
            _tiles.RemoveAt(idx);
            _tiles.Insert(idx, tile);
        }
    }

    private async void OnGuessClicked(object sender, EventArgs e)
    {
        if (_secret == null)
        {
            await DisplayAlert("Ops", "Nenhum aluno secreto sorteado.", "OK");
            return;
        }

        var guess = GuessEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(guess))
        {
            await DisplayAlert("Ops", "Digite o nome do aluno.", "OK");
            return;
        }

        if (guess.Equals(_secret.Name, StringComparison.OrdinalIgnoreCase))
        {
            FeedbackLabel.Text = "?? Parabéns!!! Você acertou!";
            await DisplayAlert("Parabéns!!!", $"Era: {_secret.Name}", "OK");
        }
        else
        {
            FeedbackLabel.Text = "?? Quase! Tenta de novo.";
        }
    }
}
