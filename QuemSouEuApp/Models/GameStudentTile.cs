using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace QuemSouEuApp.Models;

public sealed class GameStudentTile : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FacePhotoPath { get; set; } = "";

    private bool _isHidden;
    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            if (_isHidden == value) return;
            _isHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFaceVisible));
            OnPropertyChanged(nameof(IsBackVisible));
        }
    }

    public bool IsFaceVisible => !IsHidden;
    public bool IsBackVisible => IsHidden;

    private Color _backColor = Colors.LightGray;
    public Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor == value) return;
            _backColor = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
