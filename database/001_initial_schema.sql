IF DB_ID(N'BankingApi') IS NULL CREATE DATABASE BankingApi;
GO
USE BankingApi;
GO

CREATE TABLE dbo.Users
(
    Id CHAR(36) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(254) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(30) NULL,
    CreatedAt DATETIMEOFFSET(7) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIMEOFFSET(7) NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE TABLE dbo.Accounts
(
    Id CHAR(36) NOT NULL CONSTRAINT PK_Accounts PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    AccountNumber CHAR(10) NOT NULL,
    Balance DECIMAL(19,4) NOT NULL CONSTRAINT DF_Accounts_Balance DEFAULT 0,
    CreatedAt DATETIMEOFFSET(7) NOT NULL CONSTRAINT DF_Accounts_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIMEOFFSET(7) NULL,
    CONSTRAINT FK_Accounts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Accounts_UserId UNIQUE (UserId),
    CONSTRAINT UQ_Accounts_AccountNumber UNIQUE (AccountNumber),
    CONSTRAINT CK_Accounts_Balance_NonNegative CHECK (Balance >= 0)
);

CREATE TABLE dbo.Transactions
(
    Id CHAR(36) NOT NULL CONSTRAINT PK_Transactions PRIMARY KEY,
    SenderAccountId CHAR(36) NOT NULL,
    RecipientAccountId CHAR(36) NOT NULL,
    Amount DECIMAL(19,4) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    CreatedAt DATETIMEOFFSET(7) NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Transactions_Sender FOREIGN KEY (SenderAccountId) REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Transactions_Recipient FOREIGN KEY (RecipientAccountId) REFERENCES dbo.Accounts(Id),
    CONSTRAINT CK_Transactions_Amount_Positive CHECK (Amount > 0),
    CONSTRAINT CK_Transactions_DifferentAccounts CHECK (SenderAccountId <> RecipientAccountId)
);
GO
CREATE INDEX IX_Transactions_Sender_CreatedAt ON dbo.Transactions(SenderAccountId, CreatedAt DESC);
CREATE INDEX IX_Transactions_Recipient_CreatedAt ON dbo.Transactions(RecipientAccountId, CreatedAt DESC);
GO
