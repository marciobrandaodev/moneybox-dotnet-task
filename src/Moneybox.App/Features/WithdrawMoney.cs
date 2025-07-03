using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            if(amount <= 0)
            {
                throw new InvalidOperationException("Withdrawal amount must be positive");
            }

            var from = this.accountRepository.GetAccountById(fromAccountId);

            if (from == null) 
            {
                throw new InvalidOperationException("Account not found");
            }

            from.Withdraw(amount);

            this.accountRepository.Update(from);

            if (from.Balance < 500m)
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }
        }
    }
}
