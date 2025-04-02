//using QRCoder;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;

//public class QrCodeService
//{
//    public byte[] GenerateQrCode(string content)
//    {
//        if (string.IsNullOrWhiteSpace(content))
//            throw new ArgumentException("Контент для QR-кода не может быть пустым.");

//        using (var qrGenerator = new QRCodeGenerator())
//        {
//            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
//            using (var qrCode = new QRCode(qrCodeData))
//            {
//                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
//                {
//                    using (var memoryStream = new MemoryStream())
//                    {
//                        qrCodeImage.Save(memoryStream, ImageFormat.Png);
//                        return memoryStream.ToArray();
//                    }
//                }
//            }
//        }
//    }
//}
