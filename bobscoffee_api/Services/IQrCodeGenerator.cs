namespace bobscoffee_api.Services
{
    public interface IQrCodeGenerator
    {
        string Generate(string data, string directoryPath, string fileName);
        byte[] GenerateQrCodeImageBytes(string data);
    }
}