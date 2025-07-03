using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set;
        }

        public decimal Withdrawn { get;set; }

        public decimal PaidIn { get; set; }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Withdrawal amount must be positive");
            }
            if (Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds to withdraw");
            }
            Balance -= amount;
            Withdrawn -= amount;
        }

        public void PayIn(decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Pay-in amount must be positive");
            }
            if (PaidIn + amount > PayInLimit)
            {
                throw new InvalidOperationException("Pay-in limit exceeded");
            }
            Balance += amount;
            PaidIn += amount;
        }
    }
}
