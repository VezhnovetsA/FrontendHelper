using QRCoder;

public class QrCodeService
{
    public byte[] Generate(string text, int pixelsPerModule = 20)
    {
        using var qrCodeGenerator = new QRCodeGenerator();
        var qrData = qrCodeGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        return new PngByteQRCode(qrData).GetGraphic(pixelsPerModule);
    }
}