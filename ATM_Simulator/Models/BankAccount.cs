using System.ComponentModel.DataAnnotations;

namespace ATM_Simulator.Models
{
    public class BankAccount
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        public string PinHash { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public int FailedPinAttempts { get; set; }      // counts wrong PINs

        public DateTime? LockedUntil { get; set; }      // if set, account is locked until this time
    }
}