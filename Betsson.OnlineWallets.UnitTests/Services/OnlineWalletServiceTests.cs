using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Data.Repositories;  
using Betsson.OnlineWallets.Data.Models;       

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
    }
}
