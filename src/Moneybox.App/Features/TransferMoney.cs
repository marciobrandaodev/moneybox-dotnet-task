using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Transfer amount must be positive");
            }

            var from = this.accountRepository.GetAccountById(fromAccountId);
            if (from == null)
            {
                throw new InvalidOperationException("From account not found");
            }

            var to = this.accountRepository.GetAccountById(toAccountId);
            if (to == null)
            {
                throw new InvalidOperationException("To account not found");
            }

            from.Withdraw(amount);            
            to.PayIn(amount);

            this.accountRepository.Update(from);
            this.accountRepository.Update(to);

            if (from.Balance < 500m)
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }

            if (Account.PayInLimit - to.PaidIn < 500m)
            {
                this.notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }
        }
    }
}
