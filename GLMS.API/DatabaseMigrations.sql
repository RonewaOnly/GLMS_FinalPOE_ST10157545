-- ============================================================
-- GLMS Part 3 — Database Migration Scripts
-- For Azure SQL / SSMS / Azure Portal Query Editor
-- ============================================================

-- MIGRATION 001: Create tables
CREATE TABLE [dbo].[Clients] (
    [Id]              INT           IDENTITY(1,1) NOT NULL,
    [Name]            NVARCHAR(150) NOT NULL,
    [ContractDetails] NVARCHAR(500) NOT NULL,
    [Region]          NVARCHAR(100) NOT NULL,
    [CreatedOn]       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [dbo].[Users] (
    [Id]           INT           IDENTITY(1,1) NOT NULL,
    [Username]     NVARCHAR(100) NOT NULL,
    [PasswordHash] NVARCHAR(500) NOT NULL,
    [Role]         NVARCHAR(50)  NOT NULL DEFAULT 'Admin',
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [dbo].[Contracts] (
    [Id]                      INT           IDENTITY(1,1) NOT NULL,
    [ClientId]                INT           NOT NULL,
    [StartDate]               DATETIME2     NOT NULL,
    [EndDate]                 DATETIME2     NOT NULL,
    [Status]                  NVARCHAR(50)  NOT NULL DEFAULT 'Draft',
    [ServiceLevel]            NVARCHAR(200) NOT NULL,
    [SignedAgreementPath]     NVARCHAR(500) NULL,
    [SignedAgreementFileName] NVARCHAR(260) NULL,
    [CreatedOn]               DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Contracts]         PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_Clients] FOREIGN KEY ([ClientId])
        REFERENCES [dbo].[Clients]([Id]) ON DELETE NO ACTION
);
GO
CREATE INDEX [IX_Contracts_ClientId] ON [dbo].[Contracts] ([ClientId]);
GO

CREATE TABLE [dbo].[ServiceRequests] (
    [Id]               INT            IDENTITY(1,1) NOT NULL,
    [ContractId]       INT            NOT NULL,
    [Description]      NVARCHAR(1000) NOT NULL,
    [CostUsd]          DECIMAL(18,2)  NOT NULL,
    [CostZar]          DECIMAL(18,2)  NOT NULL,
    [ExchangeRateUsed] DECIMAL(18,4)  NOT NULL,
    [Status]           NVARCHAR(50)   NOT NULL DEFAULT 'Pending',
    [CreatedOn]        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ServiceRequests]           PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_Contracts] FOREIGN KEY ([ContractId])
        REFERENCES [dbo].[Contracts]([Id]) ON DELETE NO ACTION
);
GO
CREATE INDEX [IX_ServiceRequests_ContractId] ON [dbo].[ServiceRequests] ([ContractId]);
GO

PRINT 'Migration 001: Tables created.';
GO

-- MIGRATION 002: Seed data
INSERT INTO [dbo].[Users] ([Username],[PasswordHash],[Role]) VALUES ('admin','Admin@1234','Admin');
GO

INSERT INTO [dbo].[Clients] ([Name],[ContractDetails],[Region],[CreatedOn]) VALUES
    ('Acme Freight Ltd',    'Air & sea freight', 'EMEA', '2024-01-01'),
    ('FastTrack Logistics', 'Road haulage',      'SADC', '2024-01-01'),
    ('Global Ship Co',      'Ocean freight',     'APAC', '2024-01-01');
GO

INSERT INTO [dbo].[Contracts] ([ClientId],[StartDate],[EndDate],[Status],[ServiceLevel],[CreatedOn]) VALUES
    (1, '2024-01-01', '2025-01-01', 'Active',  'Priority 1 — 4-hour response',  '2024-01-01'),
    (2, '2023-06-01', '2024-06-01', 'Expired', 'Standard — 24-hour response',   '2023-06-01');
GO

PRINT 'Migration 002: Seed data inserted.';
GO

-- MIGRATION 003: EF Migrations history table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='__EFMigrationsHistory')
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO
INSERT INTO [dbo].[__EFMigrationsHistory] VALUES ('20240101000001_InitialCreate','8.0.0');
GO

PRINT 'All migrations complete.';
GO

/* ROLLBACK (uncomment to drop everything):
DROP TABLE IF EXISTS [dbo].[ServiceRequests];
DROP TABLE IF EXISTS [dbo].[Contracts];
DROP TABLE IF EXISTS [dbo].[Clients];
DROP TABLE IF EXISTS [dbo].[Users];
DROP TABLE IF EXISTS [dbo].[__EFMigrationsHistory];
*/
