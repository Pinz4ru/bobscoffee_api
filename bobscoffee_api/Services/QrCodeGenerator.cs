using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace bobscoffee_api.Services
{
    public class QrCodeGenerator
    {
        public static string Generate(string content, string folderPath, string fileName)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrImage = qrCode.GetGraphic(20);

            string fullPath = Path.Combine(folderPath, $"{fileName}.png");
            qrImage.Save(fullPath, ImageFormat.Png);

            return fullPath;
        }
    }
}
