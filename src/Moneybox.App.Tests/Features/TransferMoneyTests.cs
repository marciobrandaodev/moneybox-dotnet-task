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
public class TransferMoneyTests
{
    private IFixture _fixture;
    private Mock<IAccountRepository> _accountRepositoryMock;
    private Mock<INotificationService> _notificationServiceMock;
    private TransferMoney _transferMoney;
    private Account _fromAccount;
    private Account _toAccount;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        _accountRepositoryMock = _fixture.Freeze<Mock<IAccountRepository>>();
        _notificationServiceMock = _fixture.Freeze<Mock<INotificationService>>();
        _transferMoney = new TransferMoney(_accountRepositoryMock.Object, _notificationServiceMock.Object);

        _fromAccount = _fixture.Build<Account>()
            .With(a => a.Id, Guid.NewGuid())
            .With(a => a.User, _fixture.Build<User>().With(u => u.Email, "from@mail.com").Create())
            .With(a => a.Balance, 1000m)
            .With(a => a.Withdrawn, 0m)
            .With(a => a.PaidIn, 0m)
            .Create();

        _toAccount = _fixture.Build<Account>()
            .With(a => a.Id, Guid.NewGuid())
            .With(a => a.User, _fixture.Build<User>().With(u => u.Email, "to@mail.com").Create())
            .With(a => a.Balance, 500m)
            .With(a => a.Withdrawn, 0m)
            .With(a => a.PaidIn, 0m)
            .Create();
    }

    [Test]
    public void Execute_SuccessfulTransfer_UpdatesAccounts()
    {
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);

        _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m);

        Assert.AreEqual(800m, _fromAccount.Balance);
        Assert.AreEqual(-200m, _fromAccount.Withdrawn);
        Assert.AreEqual(700m, _toAccount.Balance);
        Assert.AreEqual(200m, _toAccount.PaidIn);

        _accountRepositoryMock.Verify(r => r.Update(_fromAccount), Times.Once);
        _accountRepositoryMock.Verify(r => r.Update(_toAccount), Times.Once);
    }

    [Test]
    public void Execute_InsufficientFunds_ThrowsInvalidOperationException()
    {
        _fromAccount.Balance = 100m;
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);

        Assert.Throws<InvalidOperationException>(() =>
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m));
    }

    [Test]
    public void Execute_LowFunds_NotifiesUser()
    {
        _fromAccount.Balance = 600m;
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);

        _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m);

        _notificationServiceMock.Verify(
            n => n.NotifyFundsLow(_fromAccount.User.Email), Times.Once);
    }

    [Test]
    public void Execute_PayInLimitExceeded_ThrowsInvalidOperationException()
    {
        _toAccount.PaidIn = 3900m;
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);

        Assert.Throws<InvalidOperationException>(() =>
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m));
    }

    [Test]
    public void Execute_ApproachingPayInLimit_NotifiesUser()
    {
        _toAccount.PaidIn = 3600m;
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);

        _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m);

        _notificationServiceMock.Verify(
            n => n.NotifyApproachingPayInLimit(_toAccount.User.Email), Times.Once);
    }

    [Test]
    public void Execute_FromAccountNotFound_ThrowsInvalidOperationException()
    {
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns((Account)null);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);

        Assert.Throws<InvalidOperationException>(() =>
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m));
    }

    [Test]
    public void Execute_ToAccountNotFound_ThrowsInvalidOperationException()
    {
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns((Account)null);
        
        Assert.Throws<InvalidOperationException>(() =>
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 200m));
    }

    [Test]
    public void Execute_ZeroOrNegativeAmount_ThrowsInvalidOperationException()
    {
        _accountRepositoryMock.Setup(r => r.GetAccountById(_fromAccount.Id)).Returns(_fromAccount);
        _accountRepositoryMock.Setup(r => r.GetAccountById(_toAccount.Id)).Returns(_toAccount);
        
        Assert.Throws<InvalidOperationException>(() =>
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, 0m));
        
        Assert.Throws<InvalidOperationException>(() =>
            _transferMoney.Execute(_fromAccount.Id, _toAccount.Id, -100m));
    }

}