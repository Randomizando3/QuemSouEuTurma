using System.Collections.ObjectModel;
using System.Linq;
using QuemSouEuApp.Models;
using QuemSouEuApp.Services;
using SkiaSharp;

namespace QuemSouEuApp.Views;

[QueryProperty(nameof(ClassPhotoPath), "path")]
public partial class ClassFaceMarkingPage : ContentPage
{
    public string ClassPhotoPath { get; set; } = "";

    private RectF _rect;
    private PointF _start;
    private bool _isDown;

    private readonly ObservableCollection<Student> _students = new();

    // dimensões reais da imagem (pixels)
    private int _imgW;
    private int _imgH;

    public ClassFaceMarkingPage()
    {
        InitializeComponent();

        StudentsList.ItemsSource = _students;

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

        if (string.IsNullOrWhiteSpace(ClassPhotoPath) || !File.Exists(ClassPhotoPath))
            return;

        // ? lê bytes de forma segura (retry + FileShare.ReadWrite)
        var bytes = await FileSafeReadService.ReadAllBytesWithRetryAsync(ClassPhotoPath);

        // ? preview sem lock no arquivo
        PhotoImg.Source = ImageSource.FromStream(() => new MemoryStream(bytes));

        // ? dimensões reais sem lock
        using var ms = new MemoryStream(bytes);
        using var codec = SKCodec.Create(ms);
        _imgW = codec.Info.Width;
        _imgH = codec.Info.Height;

        // UX: já deixa pronto para digitar o primeiro nome
        MainThread.BeginInvokeOnMainThread(() => NameEntry.Focus());
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

        // ? converte rect do overlay (com AspectFit) para pixels do bitmap
        var crop = ConvertOverlayRectToBitmapRect(_rect);

        string facePath;
        try
        {
            facePath = ImageCropService.CropAndSave(
                ClassPhotoPath,
                crop,
                $"face_{Guid.NewGuid():N}"
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", $"Falha ao recortar: {ex.Message}", "OK");
            return;
        }

        _students.Add(new Student
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            ClassPhotoPath = ClassPhotoPath,
            FacePhotoPath = facePath,
            Face = new FaceRegion
            {
                X = _rect.X,
                Y = _rect.Y,
                Width = _rect.Width,
                Height = _rect.Height
            }
        });

        // limpa pro próximo
        NameEntry.Text = "";
        _rect = new RectF(0, 0, 0, 0);
        Overlay.Invalidate();

        // UX: já deixa o teclado pronto
        NameEntry.Focus();
    }

    // ? remove aluno da lista (botão ? usa CommandParameter=Id)
    private void OnRemoveClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not string id) return;

        var item = _students.FirstOrDefault(s => s.Id == id);
        if (item == null) return;

        _students.Remove(item);

        // opcional: tentar apagar o arquivo recortado também (não quebra se falhar)
        try
        {
            if (!string.IsNullOrWhiteSpace(item.FacePhotoPath) && File.Exists(item.FacePhotoPath))
                File.Delete(item.FacePhotoPath);
        }
        catch { /* ignore */ }
    }

    private async void OnFinishClicked(object sender, EventArgs e)
    {
        if (_students.Count == 0)
        {
            await DisplayAlert("Ops", "Adicione pelo menos 1 aluno.", "OK");
            return;
        }

        var all = await StorageService.LoadAsync();
        all.AddRange(_students);
        await StorageService.SaveAsync(all);

        // ? marca essa foto como a turma atual
        await ClassStateService.SetCurrentClassPhotoPathAsync(ClassPhotoPath);

        await Shell.Current.GoToAsync($"classsummary?path={Uri.EscapeDataString(ClassPhotoPath)}");
    }


    /// <summary>
    /// Converte o retângulo desenhado no overlay para pixels reais da imagem,
    /// levando em conta AspectFit (barras e escala). Inclui clamp dentro do bitmap.
    /// </summary>
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

        // ? clamp dentro do bitmap
        left = Math.Clamp(left, 0, _imgW - 1);
        top = Math.Clamp(top, 0, _imgH - 1);
        right = Math.Clamp(right, left + 1, _imgW);
        bottom = Math.Clamp(bottom, top + 1, _imgH);

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
