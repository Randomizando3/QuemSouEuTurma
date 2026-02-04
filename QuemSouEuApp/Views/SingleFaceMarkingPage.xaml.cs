using System.Collections.ObjectModel;
using QuemSouEuApp.Models;
using QuemSouEuApp.Services;
using SkiaSharp;

namespace QuemSouEuApp.Views;

[QueryProperty(nameof(PhotoPath), "photo")]
[QueryProperty(nameof(ClassPhotoPath), "classPath")]
public partial class SingleFaceMarkingPage : ContentPage
{
    public string PhotoPath { get; set; } = "";
    public string ClassPhotoPath { get; set; } = "";

    private RectF _rect;
    private PointF _start;
    private bool _isDown;

    private int _imgW;
    private int _imgH;

    public SingleFaceMarkingPage()
    {
        InitializeComponent();

        Overlay.Drawable = new RectDrawable(() => _rect);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerPressed += OnPointerPressed;
        pointer.PointerMoved += OnPointerMoved;
        pointer.PointerReleased += OnPointerReleased;
        Overlay.GestureRecognizers.Add(pointer);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (string.IsNullOrWhiteSpace(PhotoPath) || !File.Exists(PhotoPath))
            return;

        var bytes = await FileSafeReadService.ReadAllBytesWithRetryAsync(PhotoPath);
        PhotoImg.Source = ImageSource.FromStream(() => new MemoryStream(bytes));

        using var ms = new MemoryStream(bytes);
        using var codec = SKCodec.Create(ms);
        _imgW = codec.Info.Width;
        _imgH = codec.Info.Height;
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        var p = e.GetPosition(Overlay);
        if (p is null) return;

        _isDown = true;
        _start = new PointF((float)p.Value.X, (float)p.Value.Y);
        _rect = new RectF(_start.X, _start.Y, 1, 1);
        Overlay.Invalidate();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDown) return;

        var p = e.GetPosition(Overlay);
        if (p is null) return;

        var x2 = (float)p.Value.X;
        var y2 = (float)p.Value.Y;

        var x = Math.Min(_start.X, x2);
        var y = Math.Min(_start.Y, y2);
        var w = Math.Abs(x2 - _start.X);
        var h = Math.Abs(y2 - _start.Y);

        _rect = new RectF(x, y, w, h);
        Overlay.Invalidate();
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        _isDown = false;
        Overlay.Invalidate();
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Ops", "Digite o nome do aluno.", "OK");
            return;
        }

        if (_rect.Width < 12 || _rect.Height < 12)
        {
            await DisplayAlert("Ops", "Desenhe um retângulo no rosto.", "OK");
            return;
        }

        if (_imgW <= 0 || _imgH <= 0)
        {
            await DisplayAlert("Ops", "Não consegui ler o tamanho da imagem.", "OK");
            return;
        }

        // Converte para pixels reais do bitmap
        var crop = ConvertOverlayRectToBitmapRect(_rect);

        string facePath;
        try
        {
            facePath = ImageCropService.CropAndSave(
                PhotoPath,
                crop,
                $"face_{Guid.NewGuid():N}"
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", $"Falha ao recortar: {ex.Message}", "OK");
            return;
        }

        // Salva aluno na mesma turma (ClassPhotoPath do summary)
        var all = await StorageService.LoadAsync();
        all.Add(new Student
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            ClassPhotoPath = ClassPhotoPath,
            FacePhotoPath = facePath,
            Face = new FaceRegion { X = _rect.X, Y = _rect.Y, Width = _rect.Width, Height = _rect.Height }
        });
        await StorageService.SaveAsync(all);

        await DisplayAlert("Pronto", "Aluno adicionado à turma!", "OK");

        // volta para o summary (recarrega no OnAppearing)
        await Shell.Current.Navigation.PopAsync();
    }

    private SKRectI ConvertOverlayRectToBitmapRect(RectF overlayRect)
    {
        var vw = (float)Overlay.Width;
        var vh = (float)Overlay.Height;

        var scale = Math.Min(vw / _imgW, vh / _imgH);
        var drawnW = _imgW * scale;
        var drawnH = _imgH * scale;

        var offsetX = (vw - drawnW) / 2f;
        var offsetY = (vh - drawnH) / 2f;

        var x = (overlayRect.X - offsetX) / scale;
        var y = (overlayRect.Y - offsetY) / scale;
        var w = overlayRect.Width / scale;
        var h = overlayRect.Height / scale;

        var left = (int)MathF.Round(x);
        var top = (int)MathF.Round(y);
        var right = (int)MathF.Round(x + w);
        var bottom = (int)MathF.Round(y + h);

        return new SKRectI(left, top, right, bottom);
    }

    private sealed class RectDrawable(Func<RectF> getRect) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var r = getRect();
            if (r.Width <= 1 || r.Height <= 1) return;

            canvas.FillColor = new Color(1f, 0f, 0f, 0.15f);
            canvas.FillRectangle(r);

            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 4;
            canvas.DrawRectangle(r);
        }
    }
}
