using SkiaSharp;

namespace QuemSouEuApp.Services;

public static class ImageCropService
{
    public static string CropAndSave(string sourcePath, SKRectI cropRect, string outputFileNameNoExt)
    {
        using var input = File.OpenRead(sourcePath);
        using var codec = SKCodec.Create(input);
        using var original = SKBitmap.Decode(codec);

        // limita dentro da imagem
        var safe = new SKRectI(
            Math.Max(0, cropRect.Left),
            Math.Max(0, cropRect.Top),
            Math.Min(original.Width, cropRect.Right),
            Math.Min(original.Height, cropRect.Bottom)
        );

        var width = safe.Width;
        var height = safe.Height;

        if (width <= 1 || height <= 1)
            throw new InvalidOperationException("Crop inválido.");

        using var cropped = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(cropped))
        {
            var src = new SKRect(safe.Left, safe.Top, safe.Right, safe.Bottom);
            var dst = new SKRect(0, 0, width, height);
            canvas.DrawBitmap(original, src, dst);
            canvas.Flush();
        }

        var outPath = Path.Combine(FileSystem.AppDataDirectory, $"{outputFileNameNoExt}.png");
        using var image = SKImage.FromBitmap(cropped);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var outStream = File.OpenWrite(outPath);
        data.SaveTo(outStream);

        return outPath;
    }
}
