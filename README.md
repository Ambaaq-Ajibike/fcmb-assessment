# FCMB Banking API Assessment

A secure REST API for user registration, authentication, account management, fund transfers, and transaction history. The project uses ASP.NET Core, Dapper, SQL Server, JWT bearer authentication, FluentValidation, Swagger, and xUnit.

## Features

- User registration with securely hashed passwords
- JWT-based login and endpoint authorization
- Account information and balance retrieval
- Secure profile and contact-information updates
- Atomic account-to-account fund transfers
- Paginated debit and credit transaction history
- FluentValidation request validation
- RFC 7807 Problem Details error responses
- Swagger/OpenAPI documentation
- Unit tests for services, password hashing, and validators

## Technology

- .NET 10 and ASP.NET Core Web API
- Dapper
- Microsoft SQL Server
- Microsoft.Data.SqlClient
- JWT bearer authentication
- FluentValidation
- Swashbuckle/Swagger
- xUnit

## Project structure

```text
database/
  001_initial_schema.sql       Database schema
src/BankingApi.Api/
  Contracts/                   API request and response contracts
  Controllers/                 HTTP endpoints
  Data/                        SQL connection factory
  Exceptions/                  Application exceptions
  Filters/                     FluentValidation MVC integration
  Middleware/                  Global exception handling
  Models/                      Entities and operation results
  Options/                     JWT configuration
  Repositories/                Dapper persistence operations
  Services/                    Authentication and banking logic
  Validators/                  FluentValidation validators
tests/BankingApi.Tests/         Automated tests
```

## Configuration

The application reads configuration from ASP.NET Core configuration providers. For local development, set the following values using environment variables or .NET user secrets:

| Setting | Environment variable | Description |
|---|---|---|
| `ConnectionStrings:BankingDatabase` | `ConnectionStrings__BankingDatabase` | SQL Server connection string |
| `Jwt:Issuer` | `Jwt__Issuer` | Expected JWT issuer |
| `Jwt:Audience` | `Jwt__Audience` | Expected JWT audience |
| `Jwt:Key` | `Jwt__Key` | Signing key of at least 32 bytes |
| `Jwt:ExpiryMinutes` | `Jwt__ExpiryMinutes` | Token lifetime in minutes |

Example using PowerShell environment variables:

```powershell
$env:ConnectionStrings__BankingDatabase = "Server=localhost;Database=BankingApi;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__Key = "replace-with-a-private-key-at-least-32-bytes-long"
```

## Database schema

Execute [database/001_initial_schema.sql](database/001_initial_schema.sql) against the target SQL Server instance before starting the API.

The script creates:

- `Users` for identity, profile, and password-hash data
- `Accounts` for account numbers and current balances
- `Transactions` for completed transfer records
- Foreign keys, unique constraints, balance checks, and transaction-history indexes

Every newly registered account starts with a zero balance. SQL Server applies this through the `DF_Accounts_Balance` default constraint.

### Relationships

- A user has one account, enforced by the unique `Accounts.UserId` constraint.
- An account can send many transactions.
- An account can receive many transactions.
- Each transaction has exactly one sender account and one recipient account.

## Run the application

Restore dependencies, run the tests, and start the API:

```powershell
dotnet restore
dotnet test
dotnet run --project src/BankingApi.Api
```

In the Development environment, Swagger UI is available at `/swagger` on the application URL printed in the console.

## Authentication

Registration and login are anonymous endpoints. All account and transaction endpoints require a valid bearer token.

Send the token in the HTTP header:

```http
Authorization: Bearer <access-token>
```

Swagger UI also provides an **Authorize** button for entering the token.

## API endpoints

### Register a user

`POST /api/auth/register`

```json
{
  "fullName": "Ada Okafor",
  "email": "ada@example.com",
  "password": "SecurePass1"
}
```

Returns `201 Created` with the user ID, account number, access token, and token expiration time.

### Log in

`POST /api/auth/login`

```json
{
  "email": "ada@example.com",
  "password": "SecurePass1"
}
```

Returns `200 OK` with an access token and account information. Invalid credentials return `401 Unauthorized`.

### Retrieve the current account

`GET /api/accounts/me`

Requires authentication. Returns the current user's profile, account number, and balance.

### Update the current profile

`PUT /api/accounts/me`

```json
{
  "fullName": "Adaeze Okafor",
  "phoneNumber": "+234 800 000 0000"
}
```

Requires authentication. Only the identity represented by the JWT can update its profile.

### Transfer funds

`POST /api/transactions/transfer`

```json
{
  "recipientAccountNumber": "1234567890",
  "amount": 250.00,
  "description": "Invoice payment"
}
```

Requires authentication. The API rejects:

- Invalid account numbers or amounts
- Missing recipient accounts
- Transfers to the sender's own account
- Transfers with insufficient funds

The debit, credit, and transaction record are committed together in a serializable SQL transaction. Any failure rolls back the complete operation.

### Retrieve transaction history

`GET /api/transactions?page=1&pageSize=20`

Requires authentication. Results are ordered from newest to oldest and identify each entry as a debit or credit. `pageSize` must be between 1 and 100.

## Error responses

Application errors use the Problem Details format:

```json
{
  "title": "The account has insufficient funds.",
  "status": 400,
  "code": "insufficient_funds",
  "traceId": "request-trace-identifier"
}
```

Common error codes include:

| Code | Status | Meaning |
|---|---:|---|
| `invalid_credentials` | 401 | Email or password is incorrect |
| `email_exists` | 409 | The email is already registered |
| `account_not_found` | 404 | The sender or current account does not exist |
| `recipient_not_found` | 404 | The recipient account does not exist |
| `self_transfer` | 400 | Sender and recipient are the same account |
| `insufficient_funds` | 400 | The sender does not have enough money |
| `invalid_pagination` | 400 | Page parameters are outside the permitted range |

Validation failures return `400 Bad Request` with errors grouped by request property.

## Security design

- Passwords are processed by ASP.NET Core's versioned password hasher and are never stored in plaintext.
- JWT signatures, issuer, audience, expiration, and signing keys are validated.
- Protected endpoints derive the user ID from the JWT subject rather than accepting it from request input.
- All Dapper SQL uses parameters.
- Account creation and fund transfers use SQL transactions.
- Transfers lock relevant accounts, conditionally debit available funds, credit the recipient, and record the transaction atomically.
- Database constraints prevent negative balances, non-positive transfers, duplicate emails, duplicate account numbers, and self-referencing transactions.

## Tests

Run all automated tests with:

```powershell
dotnet test BankingApi.slnx
```

The current suite covers:

- Password hashing and password verification
- Positive transfer-response mapping
- Invalid transaction-history pagination
- Registration validation
- Account-number validation
- Transfer-amount validation
- Phone-number validation

## Interview demonstration flow

Because new accounts start at zero, fund a demonstration account through an authorized database seed or administrative process before demonstrating a transfer. Do not expose a public deposit endpoint solely for the demonstration.

Suggested flow:

1. Register two users.
2. Log in and authorize Swagger as the first user.
3. Retrieve the first user's account.
4. Fund the first account through the controlled demonstration process.
5. Transfer funds to the second user's account number.
6. Show the updated balances for both accounts.
7. Show debit and credit transaction histories.
8. Demonstrate invalid login, self-transfer, missing recipient, and insufficient-funds responses.
