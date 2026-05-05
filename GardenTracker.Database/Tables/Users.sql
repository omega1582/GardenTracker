CREATE TABLE [dbo].[Users]
(
    [Id]                   INT            NOT NULL IDENTITY(1,1),
    [Email]                NVARCHAR(256)  NOT NULL,
    [PasswordHash]         NVARCHAR(MAX)  NOT NULL,
    [DisplayName]          NVARCHAR(100)  NOT NULL,
    [CreatedAt]            DATETIME2      NOT NULL,
    [RefreshToken]         NVARCHAR(MAX)  NULL,
    [RefreshTokenExpiry]   DATETIME2      NULL,

    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);
