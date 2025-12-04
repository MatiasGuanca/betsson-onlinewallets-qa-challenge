using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestSharp;

namespace Betsson.OnlineWallets.ApiTests
{
    public class WalletApiTests
    {
        private const string BaseUrl = "http://localhost:8080";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RestClient _client;

        public WalletApiTests()
        {
            _client = new RestClient(BaseUrl);
        }

        private record BalanceResponse(decimal Amount);
        private record DepositRequest(decimal Amount);
        private record WithdrawalRequest(decimal Amount);

        private static T Deserialize<T>(string json) =>
            JsonSerializer.Deserialize<T>(json, JsonOptions)!;

        [Fact]
        public async Task GetBalance_ShouldReturnOkAndValidAmount()
        {
            var request = new RestRequest("/onlinewallet/balance", Method.Get);

            var response = await _client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNullOrWhiteSpace();

            var balance = Deserialize<BalanceResponse>(response.Content!);
            balance.Should().NotBeNull();
        }

        [Fact]
        public async Task Deposit_ShouldReturnNewBalance()
        {
            var getBalance = new RestRequest("/onlinewallet/balance", Method.Get);
            var initialResponse = await _client.ExecuteAsync(getBalance);
            initialResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var initialBalance = Deserialize<BalanceResponse>(initialResponse.Content!);
            var depositAmount = 30m;

            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post)
                .AddJsonBody(new DepositRequest(depositAmount));

            var depositResponse = await _client.ExecuteAsync(depositRequest);

            depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            depositResponse.Content.Should().NotBeNullOrWhiteSpace();

            var balanceAfterDeposit = Deserialize<BalanceResponse>(depositResponse.Content!);

            balanceAfterDeposit.Amount.Should().Be(initialBalance.Amount + depositAmount);
        }

        [Fact]
        public async Task Withdraw_WithAmountGreaterThanBalance_ShouldReturnBadRequest()
        {
            var getBalance = new RestRequest("/onlinewallet/balance", Method.Get);
            var balanceResp = await _client.ExecuteAsync(getBalance);
            balanceResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var balance = Deserialize<BalanceResponse>(balanceResp.Content!);
            var withdrawAmount = balance.Amount + 999m;

            var withdraw = new RestRequest("/onlinewallet/withdraw", Method.Post)
                .AddJsonBody(new WithdrawalRequest(withdrawAmount));

            var withdrawResp = await _client.ExecuteAsync(withdraw);

            withdrawResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            withdrawResp.Content.Should().NotBeNullOrWhiteSpace();
        }
    }
}
