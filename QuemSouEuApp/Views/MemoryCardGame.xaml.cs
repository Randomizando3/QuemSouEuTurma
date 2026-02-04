using System.Collections.ObjectModel;
using QuemSouEuApp.Models;
using QuemSouEuApp.Services;

namespace QuemSouEuApp.Views;

public partial class MemoryCardGame : ContentPage
{
    private readonly ObservableCollection<MemoryCardTile> _cards = new();

    private MemoryCardTile? _first;
    private MemoryCardTile? _second;
    private bool _lock;

    private int _selectedPairs = 2;

    public MemoryCardGame()
    {
        InitializeComponent();
        CardsGrid.ItemsSource = _cards;

        ConfigPairsLabel.Text = "2";
        ConfigOverlay.IsVisible = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Fluxo padrão de jogo: sempre pede configuração ao entrar
        ConfigOverlay.IsVisible = true;
    }

    // =========================
    // CONFIGURAÇÃO
    // =========================

    private void OnConfigPairsChanged(object sender, ValueChangedEventArgs e)
    {
        _selectedPairs = (int)Math.Round(e.NewValue);
        if (_selectedPairs < 2) _selectedPairs = 2;
        if (_selectedPairs > 10) _selectedPairs = 10;

        ConfigPairsLabel.Text = _selectedPairs.ToString();
    }

    private async void OnGenerateGameClicked(object sender, EventArgs e)
    {
        ConfigOverlay.IsVisible = false;
        await StartGameAsync();
    }

    // =========================
    // JOGO
    // =========================

    private async Task StartGameAsync()
    {
        _cards.Clear();
        _first = null;
        _second = null;
        _lock = false;

        var (_, students) = await GameService.LoadCurrentClassAsync();

        if (students.Count < 2)
        {
            await ShowToastAsync("Cadastre pelo menos 2 alunos.");
            ConfigOverlay.IsVisible = true;
            return;
        }

        // máximo de pares = min(10, total alunos)
        var maxPairs = Math.Min(10, students.Count);
        var pairs = Math.Clamp(_selectedPairs, 2, maxPairs);

        // garante que o slider respeite o máximo real da turma (se turma tiver menos alunos)
        ConfigPairsSlider.Maximum = maxPairs;
        if (ConfigPairsSlider.Value > maxPairs) ConfigPairsSlider.Value = maxPairs;

        var selected = MemoryGameService.PickRandomStudents(students, pairs);
        var deck = MemoryGameService.BuildDeck(selected);

        foreach (var c in deck)
            _cards.Add(c);

        await ShowToastAsync($"Jogo iniciado com {pairs} pares!");
    }

    // =========================
    // CARTAS
    // =========================

    private async void OnCardTapped(object sender, TappedEventArgs e)
    {
        if (_lock) return;

        if (sender is not VisualElement ve) return;
        if (ve.BindingContext is not MemoryCardTile card) return;

        if (card.IsMatched || card.IsFlipped) return;

        // vira a carta
        card.IsFlipped = true;

        if (_first == null)
        {
            _first = card;
            return;
        }

        if (_second != null) return;

        _second = card;
        _lock = true;

        // ? delay visual para a criança ver a 2ª carta
        await Task.Delay(700);

        if (_first.PairKey == _second.PairKey)
        {
            _first.IsMatched = true;
            _second.IsMatched = true;
            await ShowToastAsync("Par encontrado!");
        }
        else
        {
            _first.IsFlipped = false;
            _second.IsFlipped = false;
        }

        _first = null;
        _second = null;
        _lock = false;

        if (_cards.Count > 0 && _cards.All(c => c.IsMatched))
        {
            await ShowToastAsync("?? Você venceu! Parabéns!");
        }
    }

    // =========================
    // BOTÕES
    // =========================

    private void OnRestartClicked(object sender, EventArgs e)
    {
        // abre o modal de configuração (mais prático)
        ConfigOverlay.IsVisible = true;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (Shell.Current?.Navigation?.NavigationStack?.Count > 1)
            await Shell.Current.Navigation.PopAsync();
        else
            await Shell.Current.GoToAsync("///home");
    }

    // =========================
    // TOAST
    // =========================

    private async Task ShowToastAsync(string message)
    {
        ToastLabel.Text = message;
        Toast.IsVisible = true;
        Toast.Opacity = 0;

        await Toast.FadeTo(1, 140);
        await Task.Delay(1200);
        await Toast.FadeTo(0, 160);

        Toast.IsVisible = false;
    }
}
