using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuemSouEuApp.Models;

public sealed class MemoryCardTile : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    // Mesmo PairKey = par
    public string PairKey { get; set; } = "";

    // FacePhotoPath do aluno
    public string FacePhotoPath { get; set; } = "";

    private bool _isFlipped;
    public bool IsFlipped
    {
        get => _isFlipped;
        set
        {
            if (_isFlipped == value) return;
            _isFlipped = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFaceVisible));
            OnPropertyChanged(nameof(IsBackVisible));
        }
    }

    private bool _isMatched;
    public bool IsMatched
    {
        get => _isMatched;
        set
        {
            if (_isMatched == value) return;
            _isMatched = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CardOpacity));
        }
    }

    public bool IsFaceVisible => IsFlipped || IsMatched;
    public bool IsBackVisible => !IsFaceVisible;

    public double CardOpacity => IsMatched ? 0.55 : 1.0;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
