CREATE DATABASE CurrencyWalletDb;
GO

USE CurrencyWalletDb;
GO


CREATE TABLE dbo.CurrencyCodes
(
    Code varchar(3)                     NOT NULL,
    Id   int PRIMARY KEY IDENTITY (1,1) NOT NULL
);

CREATE TABLE dbo.CurrencyRates
(
    Id             uniqueidentifier PRIMARY KEY DEFAULT (newid()) NOT NULL,
    RateDate       date                                           NOT NULL,
    Rate           decimal(18, 5)                                 NOT NULL,
    CurrencyCodeId int                                            NOT NULL,
    FOREIGN KEY (CurrencyCodeId) REFERENCES CurrencyCodes (Id)
);

CREATE TABLE dbo.Wallets
(
    Id             uniqueidentifier PRIMARY KEY DEFAULT (newid()) NOT NULL,
    Balance        decimal(18, 2)               DEFAULT ((0)),
    CurrencyCodeId int                                            NOT NULL,
    FOREIGN KEY (CurrencyCodeId) REFERENCES CurrencyCodes (Id)
);

-- Create indexes
CREATE UNIQUE INDEX IX_CurrencyCodes_Code ON dbo.CurrencyCodes (Code);
CREATE INDEX IX_CurrencyRates_CurrencyCodeId ON dbo.CurrencyRates (CurrencyCodeId);
CREATE INDEX IX_CurrencyRates_RateDate ON dbo.CurrencyRates (RateDate);
CREATE INDEX IX_CurrencyRates_CurrencyCodeId_RateDate ON dbo.CurrencyRates (CurrencyCodeId, RateDate);
CREATE INDEX IX_CurrencyRates_CurrencyCodeId_RateDate_Desc ON dbo.CurrencyRates (CurrencyCodeId, RateDate DESC);
CREATE INDEX IX_Wallets_CurrencyCodeId ON dbo.Wallets (CurrencyCodeId);
CREATE INDEX IX_Wallets_Balance ON dbo.Wallets (Balance);
