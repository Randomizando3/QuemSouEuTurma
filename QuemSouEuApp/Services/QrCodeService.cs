using QRCoder;

namespace QuemSouEuApp.Services;

public static class QrCodeService
{
    // Retorna PNG bytes com o QR do texto (nome do aluno)
    public static byte[] GeneratePng(string text, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text ?? "", QRCodeGenerator.ECCLevel.Q);

        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }
}
