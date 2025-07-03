using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace Moneybox.App.Tests.Features;

[TestFixture]
public class WithdrawMoneyTests
{
    private IFixture _fixture;
    private Mock<IAccountRepository> _accountRepositoryMock;
    private Mock<INotificationService> _notificationServiceMock;
    private WithdrawMoney _withdrawMoney;
    private Account _fromAccount;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        _accountRepositoryMock = _fixture.Freeze<Mock<IAccountRepository>>();
        _notificationServiceMock = _fixture.Freeze<Mock<INotificationService>>();
        _withdrawMoney = new WithdrawMoney(_accountRepositoryMock.Object, _notificationServiceMock.Object);

        _fromAccount = _fixture.Build<Account>()
            .With(a => a.Id, Guid.NewGuid())
            .With(a => a.User, _fixture.Build<User>().With(u => u.Email, "from@mail.com").Create())
            .With(a => a.Balance, 1000m)
            .With(a => a.Withdrawn, 0m)
            .Create();
    }

    [Test]
    public void Execute_SuccessfulWithdraw_UpdatesAccount()
    {
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);

        _withdrawMoney.Execute(_fromAccount.Id, 200m);

        Assert.AreEqual(800m, _fromAccount.Balance);
        Assert.AreEqual(-200m, _fromAccount.Withdrawn);

        _accountRepositoryMock.Verify(r => r.Update(_fromAccount), Times.Once);
    }

    [Test]
    public void Execute_InsufficientFunds_ThrowsInvalidOperation()
    {
        _fromAccount.Balance = 100m;
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);

        Assert.Throws<InvalidOperationException>(() =>
            _withdrawMoney.Execute(_fromAccount.Id, 200m));
    }

    [Test]
    public void Execute_LowFunds_NotifiesUser()
    {
        _fromAccount.Balance = 600m;
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);

        _withdrawMoney.Execute(_fromAccount.Id, 200m);

        _notificationServiceMock.Verify(
            n => n.NotifyFundsLow(_fromAccount.User.Email), Times.Once);
    }
}