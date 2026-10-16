using Microsoft.AspNetCore.Identity;

namespace ATM_Simulator.Services
{
    public class PinHelper
    {
        private readonly PasswordHasher<string> _hasher = new PasswordHasher<string>();

        // Turns "1234" into scrambled text we can safely store
        public string HashPin(string pin)
        {
            return _hasher.HashPassword("atm", pin);
        }

        // Checks if the PIN typed by the user matches the stored scrambled one
        public bool VerifyPin(string hashedPin, string enteredPin)
        {
            var result = _hasher.VerifyHashedPassword("atm", hashedPin, enteredPin);
            return result != PasswordVerificationResult.Failed;
        }
    }
}