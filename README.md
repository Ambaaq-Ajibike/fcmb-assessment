# Banking API

A secure ASP.NET Core REST API built with .NET 10, Dapper, SQL Server, JWT bearer authentication, Swagger, and xUnit.

## Configure and run

1. Execute `database/001_initial_schema.sql` against your SQL Server instance.
2. Set `ConnectionStrings__BankingDatabase` and `Jwt__Key` with environment variables or .NET user secrets. The JWT key must be at least 32 bytes.
3. Run `dotnet restore`, `dotnet test`, then `dotnet run --project src/BankingApi.Api`.
4. In Development, open `/swagger` on the URL printed by the application.

`Banking:OpeningBalance` is `1000.00` for a self-contained interview demonstration. Set it to `0` outside the demo environment.

## API flow

- `POST /api/auth/register` creates a user and account.
- `POST /api/auth/login` returns a JWT.
- Use Swagger's **Authorize** button with the JWT.
- `GET /api/accounts/me` returns the signed-in user's account.
- `PUT /api/accounts/me` updates the user's name and phone number.
- `POST /api/transactions/transfer` transfers funds to another 10-digit account number.
- `GET /api/transactions?page=1&pageSize=20` returns paginated debit and credit history.

## Demo

Register two users, copy the second user's account number, authorize as the first user, make a transfer, and then show both account balances and transaction histories. Also demonstrate an invalid login, insufficient funds, and a self-transfer to show the error responses.

## Design notes

- Passwords use ASP.NET Core's versioned password hasher and are never stored in plaintext.
- Transfers run in a serializable SQL transaction. Both accounts are locked in ID order, the debit is conditional on sufficient balance, the credit and transaction record commit together, and rollback is automatic on failure.
- SQL is parameterized throughout. Error responses use Problem Details with stable error codes and trace IDs.
- Database changes are maintained as ordered SQL scripts in `database/`; Entity Framework Core is not used.
