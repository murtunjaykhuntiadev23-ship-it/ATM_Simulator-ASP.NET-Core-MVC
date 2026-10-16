using ATM_Simulator.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ATM_Simulator.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<AtmTransaction> AtmTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BankAccount>()
                .Property(b => b.Balance)
                .HasPrecision(18, 2);

            builder.Entity<AtmTransaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            builder.Entity<AtmTransaction>()
                .Property(t => t.BalanceAfter)
                .HasPrecision(18, 2);
        }
    }
}