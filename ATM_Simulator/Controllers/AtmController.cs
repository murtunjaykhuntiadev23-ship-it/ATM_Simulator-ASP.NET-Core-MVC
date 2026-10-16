using ATM_Simulator.Data;
using ATM_Simulator.Models;
using ATM_Simulator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ATM_Simulator.Controllers
{
    [Authorize]
    public class AtmController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PinHelper _pinHelper;

        public AtmController(ApplicationDbContext context, PinHelper pinHelper)
        {
            _context = context;
            _pinHelper = pinHelper;
        }

        // Finds the bank account of the logged-in user
        private async Task<BankAccount?> GetMyAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.BankAccounts.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<IActionResult> Index()
        {
            var account = await GetMyAccount();
            if (account == null)
            {
                return RedirectToAction("CreateAccount");
            }
            return View(account);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccount()
        {
            if (await GetMyAccount() != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            {
                ViewBag.Error = "PIN must be exactly 4 digits.";
                return View();
            }

            var account = new BankAccount
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                AccountNumber = new Random().NextInt64(1000000000, 9999999999).ToString(),
                PinHash = _pinHelper.HashPin(pin),
                Balance = 0
            };

            _context.BankAccounts.Add(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // ---------- DEPOSIT ----------
        [HttpGet]
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(decimal amount, string pin)
        {
            var account = await GetMyAccount();
            if (account == null)
            {
                return RedirectToAction("CreateAccount");
            }

            var pinError = await CheckPin(account, pin);
            if (pinError != null)
            {
                ViewBag.Error = pinError;
                return View();
            }

            if (amount <= 0)
            {
                ViewBag.Error = "Amount must be more than 0.";
                return View();
            }

            account.Balance += amount;
            _context.AtmTransactions.Add(new AtmTransaction
            {
                BankAccountId = account.Id,
                Type = "Deposit",
                Amount = amount,
                BalanceAfter = account.Balance
            });
            await _context.SaveChangesAsync();

            TempData["Message"] = $"₹{amount:N2} deposited successfully.";
            return RedirectToAction("Index");
        }

        // ---------- WITHDRAW ----------
        [HttpGet]
        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(decimal amount, string pin)
        {
            var account = await GetMyAccount();
            if (account == null)
            {
                return RedirectToAction("CreateAccount");
            }

            var pinError = await CheckPin(account, pin);
            if (pinError != null)
            {
                ViewBag.Error = pinError;
                return View();
            }

            if (amount <= 0)
            {
                ViewBag.Error = "Amount must be more than 0.";
                return View();
            }

            if (amount > account.Balance)
            {
                ViewBag.Error = "Not enough balance.";
                return View();
            }

            account.Balance -= amount;
            _context.AtmTransactions.Add(new AtmTransaction
            {
                BankAccountId = account.Id,
                Type = "Withdraw",
                Amount = amount,
                BalanceAfter = account.Balance
            });
            await _context.SaveChangesAsync();

            TempData["Message"] = $"₹{amount:N2} withdrawn successfully.";
            return RedirectToAction("Index");
        }
    

            // ---------- MINI STATEMENT ----------
        public async Task<IActionResult> Statement()
        {
            var account = await GetMyAccount();
            if (account == null)
            {
                return RedirectToAction("CreateAccount");
            }

            var list = await _context.AtmTransactions
                .Where(t => t.BankAccountId == account.Id)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToListAsync();

            return View(list);
        }

        // ---------- CHANGE PIN ----------
        [HttpGet]
        public IActionResult ChangePin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePin(string oldPin, string newPin, string confirmPin)
        {
            var account = await GetMyAccount();
            if (account == null)
            {
                return RedirectToAction("CreateAccount");
            }
            var pinError = await CheckPin(account, oldPin);
            if (pinError != null)
            {
                ViewBag.Error = pinError;
                return View();
            }             

            if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4 || !newPin.All(char.IsDigit))
            {
                ViewBag.Error = "New PIN must be exactly 4 digits.";
                return View();
            }

            if (newPin != confirmPin)
            {
                ViewBag.Error = "New PIN and confirm PIN do not match.";
                return View();
            }

            account.PinHash = _pinHelper.HashPin(newPin);
            await _context.SaveChangesAsync();

            TempData["Message"] = "PIN changed successfully.";
            return RedirectToAction("Index");
        }

        // Checks the PIN, counts wrong tries, and locks the account after 3 mistakes.
        // Returns an error message, or null if the PIN is correct.
        private async Task<string?> CheckPin(BankAccount account, string? pin)
        {
            if (account.LockedUntil != null && account.LockedUntil > DateTime.Now)
            {
                var minutes = (int)Math.Ceiling((account.LockedUntil.Value - DateTime.Now).TotalMinutes);
                return $"Account is locked. Try again in {minutes} minute(s).";
            }

            if (_pinHelper.VerifyPin(account.PinHash, pin ?? ""))
            {
                account.FailedPinAttempts = 0;
                account.LockedUntil = null;
                await _context.SaveChangesAsync();
                return null;
            }

            account.FailedPinAttempts++;
            if (account.FailedPinAttempts >= 3)
            {
                account.LockedUntil = DateTime.Now.AddMinutes(15);
                account.FailedPinAttempts = 0;
                await _context.SaveChangesAsync();
                return "Too many wrong PINs. Account locked for 15 minutes.";
            }

            await _context.SaveChangesAsync();
            return $"Wrong PIN. {3 - account.FailedPinAttempts} attempt(s) left.";
        }
    }
}