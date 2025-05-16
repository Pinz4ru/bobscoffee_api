namespace bobscoffee_api.DTOs
{
    public class CoffeeScanResponse
    {
        public string Username { get; set; } = string.Empty;
        public int CoffeeCount { get; set; }
        public bool IsFreeCoffee { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}