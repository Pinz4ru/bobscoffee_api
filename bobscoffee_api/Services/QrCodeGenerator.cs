using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace bobscoffee_api.Services
{
    public class QrCodeGenerator : IQrCodeGenerator
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<QrCodeGenerator> _logger;

        public QrCodeGenerator(IWebHostEnvironment env, ILogger<QrCodeGenerator> logger)
        {
            _env = env;
            _logger = logger;
        }

        public string Generate(string data, string directoryPath, string fileName)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, directoryPath);
                Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, $"{fileName}.png");

                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new QRCode(qrCodeData))
                using (var qrCodeImage = qrCode.GetGraphic(20))
                {
                    qrCodeImage.Save(filePath, ImageFormat.Png);
                }

                _logger.LogInformation($"Generated QR code at {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR code generation failed");
                throw;
            }
        }

        public byte[] GenerateQrCodeImageBytes(string data)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new QRCode(qrCodeData))
                using (var stream = new MemoryStream())
                {
                    var qrCodeImage = qrCode.GetGraphic(20);
                    qrCodeImage.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR code bytes generation failed");
                throw;
            }
        }
    }
}