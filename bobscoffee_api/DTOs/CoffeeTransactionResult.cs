namespace bobscoffee_api.DTOs
{
    public class CoffeeTransactionResult
    {
        public bool Success { get; }
        public string Message { get; }
        public int NewCoffeeCount { get; }
        public bool IsFreeCoffee { get; }

        public CoffeeTransactionResult(
            bool success,
            string message,
            int newCoffeeCount = 0,
            bool isFreeCoffee = false)
        {
            Success = success;
            Message = message;
            NewCoffeeCount = newCoffeeCount;
            IsFreeCoffee = isFreeCoffee;
        }
    }
}
