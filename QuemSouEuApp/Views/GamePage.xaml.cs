using System.Collections.ObjectModel;
using QuemSouEuApp.Models;
using QuemSouEuApp.Services;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Audio;


namespace QuemSouEuApp.Views;

public partial class GamePage : ContentPage
{

    private readonly IAudioManager _audioManager = AudioManager.Current;
    private IAudioPlayer? _successPlayer;

    private readonly ObservableCollection<GameStudentTile> _tiles = new();

    private Student? _secret;
    private string _classPhotoPath = "";

    private readonly Random _rng = new();

    // Paleta suave/pastel (sem vibrar)
    private static readonly Color[] PastelPalette = new[]
    {
        Color.FromArgb("#F36C6C"), // vermelho suave
        Color.FromArgb("#8BE28B"), // verde suave
        Color.FromArgb("#B98CFF"), // roxo suave
        Color.FromArgb("#FFB36B"), // laranja suave
        Color.FromArgb("#6F87FF"), // azul suave
        Color.FromArgb("#B07B6E"), // marrom suave
        Color.FromArgb("#66D6FF"), // ciano suave
        Color.FromArgb("#6B87A6"), // azul acinzentado
        Color.FromArgb("#B9F0FF"), // azul bem claro
        Color.FromArgb("#B9FF6B"), // verde-lima suave
    };

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


    private async Task PlaySuccessSoundAsync()
    {
        try
        {
            // Abre o MP3 do Resources/Raw
            var stream = await FileSystem.OpenAppPackageFileAsync("parabens.mp3");

            _successPlayer?.Stop();
            _successPlayer?.Dispose();

            _successPlayer = _audioManager.CreatePlayer(stream);
            _successPlayer.Volume = 1.0;
            _successPlayer.Play();
        }
        catch
        {
            // Se falhar, não quebra o jogo
        }
    }



    private async Task LoadClassAndStartAsync()
    {
        var (classPath, students) = await GameService.LoadCurrentClassAsync();
        _classPhotoPath = classPath;

        if (string.IsNullOrWhiteSpace(_classPhotoPath) || students.Count == 0)
        {
            _tiles.Clear();
            _secret = null;
            await ShowToastAsync("Nenhuma turma cadastrada. Cadastre a turma primeiro.");
            return;
        }

        StartNewRound(students);
        await ShowToastAsync("Toque nas cartas para ocultar e depois tente adivinhar.");
    }

    private void StartNewRound(List<Student> students)
    {
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
                IsHidden = false,
                BackColor = PickPastel()
            });
        }
    }

    private Color PickPastel()
        => PastelPalette[_rng.Next(PastelPalette.Length)];

    // ========= TOP BUTTONS =========

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (Shell.Current?.Navigation?.NavigationStack?.Count > 1)
            await Shell.Current.Navigation.PopAsync();
        else
            await Shell.Current.GoToAsync("///home");
    }

    private async void OnNewGameClicked(object sender, EventArgs e)
    {
        var (_, students) = await GameService.LoadCurrentClassAsync();
        if (students.Count == 0)
        {
            await ShowToastAsync("Cadastre uma turma primeiro.");
            return;
        }

        StartNewRound(students);
        await ShowToastAsync("Novo jogo iniciado.");
    }

    private async void OnCopyClipboardClicked(object sender, EventArgs e)
    {
        if (_secret == null)
        {
            await ShowToastAsync("Nenhum segredo sorteado.");
            return;
        }

        await Clipboard.Default.SetTextAsync(_secret.Name);
        await ShowToastAsync("Nome copiado para a área de transferência.");
    }

    private async void OnShowQrClicked(object sender, EventArgs e)
    {
        if (_secret == null)
        {
            await ShowToastAsync("Nenhum segredo sorteado.");
            return;
        }

        try
        {
            var pngBytes = QrCodeService.GeneratePng(_secret.Name, pixelsPerModule: 10);
            QrImage.Source = ImageSource.FromStream(() => new MemoryStream(pngBytes));

            QrOverlay.IsVisible = true;
            await ShowToastAsync("QR Code gerado.");
        }
        catch
        {
            await ShowToastAsync("Falha ao gerar QR Code.");
        }
    }

    private void OnCloseQrClicked(object sender, EventArgs e)
    {
        QrOverlay.IsVisible = false;
        QrImage.Source = null;
    }

    // ========= CARDS =========

    private void OnTileTapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement ve) return;
        if (ve.BindingContext is not GameStudentTile tile) return;

        tile.IsHidden = !tile.IsHidden;
    }

    // ========= TOAST =========

    private async Task ShowToastAsync(string message)
    {
        ToastLabel.Text = message;
        Toast.IsVisible = true;
        Toast.Opacity = 0;

        await Toast.FadeTo(1, 140);
        await Task.Delay(1400);
        await Toast.FadeTo(0, 160);

        Toast.IsVisible = false;
    }

    private async void OnGuessClicked(object sender, EventArgs e)
    {
        if (_secret == null)
        {
            await ShowToastAsync("Nenhum aluno secreto sorteado.");
            return;
        }

        var guess = GuessEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(guess))
        {
            await ShowToastAsync("Digite o nome do aluno.");
            return;
        }

        if (guess.Equals(_secret.Name, StringComparison.OrdinalIgnoreCase))
        {
            await PlaySuccessSoundAsync(); // ? AQUI

            await ShowToastAsync($"Parabéns! Você acertou: {_secret.Name}");

            var (_, students) = await GameService.LoadCurrentClassAsync();
            StartNewRound(students);

            GuessEntry.Text = "";
        }

        else
        {
            await ShowToastAsync("Não foi dessa vez. Tente novamente.");
        }
    }

}
