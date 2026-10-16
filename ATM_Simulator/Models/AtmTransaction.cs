namespace ATM_Simulator.Models
{
    public class AtmTransaction
    {
        public int Id { get; set; }

        public int BankAccountId { get; set; }   // which account this line belongs to

        public string Type { get; set; } = string.Empty;   // "Deposit" or "Withdraw"

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}