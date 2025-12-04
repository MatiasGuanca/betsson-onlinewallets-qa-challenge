using System;
using System.Threading.Tasks;
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
    }
}
