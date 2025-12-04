# Online Wallets – QA Backend Engineer Challenge

This repository contains the complete solution for the **QA Backend Engineer Technical Challenge**, including:

- Unit tests for service-level business logic  
- End-to-end API tests validating the public REST contract  
- Docker execution pipeline  
- A structured and professional commit history  
- A clear testing strategy applied across different layers of the system  

---

## Overview

The Online Wallets API exposes three main operations:

| Method | Endpoint                     | Description                         |
|--------|------------------------------|-------------------------------------|
| GET    | /onlinewallet/balance        | Retrieves the current wallet balance |
| POST   | /onlinewallet/deposit        | Deposits a specified amount          |
| POST   | /onlinewallet/withdraw       | Withdraws money with balance checks  |

Both **internal logic** and **public API behavior** were validated independently.

---

## 🧪 Testing Strategy

Testing was performed at two levels:

---

### 1. Unit Tests — Service Layer (`OnlineWalletService`)

Frameworks used: **xUnit**, **Moq**, **FluentAssertions**

Coverage includes:

#### `GetBalanceAsync`
- Returns **0** when no previous entries exist  
- Computes balance correctly when a previous entry is present  

#### `DepositFundsAsync`
- Rejects invalid deposit amounts  
- Inserts a correct repository entry  
- Returns updated balance  

#### `WithdrawFundsAsync`
- Throws `InsufficientBalanceException` when funds are insufficient  
- Inserts a correct withdrawal entry (negative amount)  
- Returns updated balance  

Mocking is used to isolate the service from the data layer.

---

### 2. End-to-End API Tests — REST Contract Validation

Frameworks used: **RestSharp**, **FluentAssertions**, `System.Text.Json`  

These tests interact with the real API (running via Docker), verifying:

#### `GET /onlinewallet/balance`
- Returns **200 OK**  
- Response includes a valid JSON balance object  

#### `POST /onlinewallet/deposit`
- Returns **200 OK**  
- Response includes the **new balance**  
- Balance logic validated using: initialBalance + depositAmount  

#### `POST /onlinewallet/withdraw` (valid)
- Returns **200 OK**  
- Updated balance returned  

#### `POST /onlinewallet/withdraw` (insufficient funds)
- Returns **400 BadRequest**  
- Error is correctly mapped through `SystemController.Error`  

---

## JSON Deserialization Notes

ASP.NET Core serializes properties in **camelCase**.  
Tests explicitly use:

```csharp
new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
```

Ensuring correct mapping between:

- `Amount` (C#)  
- `"amount"` (JSON)  

---

## 🧱 Project Structure

```
src/
 ├── Betsson.OnlineWallets        # Service layer
 ├── Betsson.OnlineWallets.Data   # Repository / database layer
 └── Betsson.OnlineWallets.Web    # API layer

tests/
 ├── Betsson.OnlineWallets.UnitTests   # Unit tests for service logic
 └── Betsson.OnlineWallets.ApiTests    # End-to-end API tests
```

---

## 🐳 Running the Application (Docker)

### Build image
```bash
docker build -f src/Betsson.OnlineWallets.Web/Dockerfile .
```

### Run container
```bash
docker run -p 8080:8080 <image-id>
```

### Swagger UI
```
http://localhost:8080/swagger/index.html
```

---

## 🧪 Running All Tests

```bash
dotnet test
```

Expected output:

```
Passed! - Failed: 0, Passed: X, Total: X
```

---

## 📌 Commit Strategy (Conventional Commits)

The repository follows a clean, incremental commit structure:

```
test: add unit tests for GetBalanceAsync
test: add deposit and withdraw service tests
test(api): add API test project
test(api): implement balance, deposit and withdraw API tests
test(api): fix JSON deserialization and align tests with controller behavior
```

This approach ensures professional reviewability and clarity of intent.

---

## 🎯 Final Outcome

This solution demonstrates:

- Deep understanding of business logic behavior  
- Strong separation between unit testing and API contract testing  
- Proper handling of edge cases and error scenarios  
- Fully reproducible environment using Docker  
- Modern testing tools and best practices  
- Clean, auditable commit history  

---
