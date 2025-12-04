using FluentAssertions;
using Moq;


using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Data.Repositories;  
using Betsson.OnlineWallets.Data.Models;        
using Betsson.OnlineWallets.Models;             
using Betsson.OnlineWallets.Exceptions;     

namespace Betsson.OnlineWallets.UnitTests.Services
{
    public class OnlineWalletServiceTests
    {
        private readonly Mock<IOnlineWalletRepository> _onlineWalletRepositoryMock;
        private readonly OnlineWalletService _service;

        public OnlineWalletServiceTests()
        {
            
            _onlineWalletRepositoryMock = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_onlineWalletRepositoryMock.Object);
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnZero_WhenThereAreNoEntries()
        {
            _onlineWalletRepositoryMock
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync((OnlineWalletEntry?)null);

            var result = await _service.GetBalanceAsync();

            result.Should().NotBeNull();
            result.Amount.Should().Be(0);
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnComputedBalance_WhenLastEntryExists()
        {
            var lastEntry = new OnlineWalletEntry
            {
                BalanceBefore = 100m,
                Amount = 50m
            };

            _onlineWalletRepositoryMock
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(lastEntry);

            var result = await _service.GetBalanceAsync();

            result.Amount.Should().Be(150m);
        }

        [Fact]
        public async Task DepositFundsAsync_ShouldInsertEntryWithCorrectValues_AndReturnNewBalance()
        {
            var lastEntry = new OnlineWalletEntry
            {
                BalanceBefore = 50m,
                Amount = 50m 
            };

            _onlineWalletRepositoryMock
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(lastEntry);

            _onlineWalletRepositoryMock
                .Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
                .Returns(Task.CompletedTask);

            var deposit = new Deposit { Amount = 25m }; 
            var result = await _service.DepositFundsAsync(deposit);

            result.Amount.Should().Be(125m);

            _onlineWalletRepositoryMock.Verify(
                r => r.InsertOnlineWalletEntryAsync(
                    It.Is<OnlineWalletEntry>(e =>
                        e.BalanceBefore == 100m &&   
                        e.Amount == 25m              
                    )),
                Times.Once);
        }

        [Fact]
        public async Task WithdrawFundsAsync_ShouldThrowInsufficientBalanceException_WhenAmountExceedsCurrentBalance()
        {
            var lastEntry = new OnlineWalletEntry
            {
                BalanceBefore = 0m,
                Amount = 50m
            };

            _onlineWalletRepositoryMock
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(lastEntry);

            var withdrawal = new Withdrawal { Amount = 80m };

            var act = async () => await _service.WithdrawFundsAsync(withdrawal);

            await act.Should().ThrowAsync<InsufficientBalanceException>();

            _onlineWalletRepositoryMock.Verify(
                r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()),
                Times.Never);
        }

        [Fact]
        public async Task WithdrawFundsAsync_ShouldInsertNegativeEntryAndReturnNewBalance_WhenFundsAreSufficient()
        {
            var lastEntry = new OnlineWalletEntry
            {
                BalanceBefore = 0m,
                Amount = 200m
            };

            _onlineWalletRepositoryMock
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(lastEntry);

            _onlineWalletRepositoryMock
                .Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
                .Returns(Task.CompletedTask);

            var withdrawal = new Withdrawal { Amount = 80m };

            var result = await _service.WithdrawFundsAsync(withdrawal);

            result.Amount.Should().Be(120m);

            _onlineWalletRepositoryMock.Verify(
                r => r.InsertOnlineWalletEntryAsync(
                    It.Is<OnlineWalletEntry>(e =>
                        e.BalanceBefore == 200m &&
                        e.Amount == -80m
                    )),
                Times.Once);
        }
    }
}
