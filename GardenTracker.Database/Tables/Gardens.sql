CREATE TABLE [dbo].[Gardens]
(
    [Id]        INT            NOT NULL IDENTITY(1,1),
    [UserId]    INT            NOT NULL,
    [Name]      NVARCHAR(150)  NOT NULL,
    [Location]  NVARCHAR(250)  NULL,
    [Notes]     NVARCHAR(MAX)  NULL,
    [CreatedAt] DATETIME2      NOT NULL,

    CONSTRAINT [PK_Gardens]          PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Gardens_Users]    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_Gardens_UserId] ON [dbo].[Gardens] ([UserId]);
