CREATE TABLE [dbo].[Expenses]
(
    [Id]          INT            NOT NULL IDENTITY(1,1),
    [SeasonId]    INT            NOT NULL,
    [BedId]       INT            NULL,
    [SupplierId]  INT            NULL,
    [Category]    INT            NOT NULL,
    [Description] NVARCHAR(500)  NOT NULL,
    [Amount]      DECIMAL(10,2)  NOT NULL,
    [ExpenseDate] DATE           NOT NULL,

    CONSTRAINT [PK_Expenses]             PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Expenses_Seasons]     FOREIGN KEY ([SeasonId])   REFERENCES [dbo].[Seasons]    ([Id]),
    CONSTRAINT [FK_Expenses_RaisedBeds]  FOREIGN KEY ([BedId])      REFERENCES [dbo].[RaisedBeds] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Expenses_Suppliers]   FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Suppliers]  ([Id]) ON DELETE SET NULL
);

GO

CREATE INDEX [IX_Expenses_SeasonId] ON [dbo].[Expenses] ([SeasonId]);

GO

CREATE INDEX [IX_Expenses_BedId] ON [dbo].[Expenses] ([BedId]);

GO

CREATE INDEX [IX_Expenses_SupplierId] ON [dbo].[Expenses] ([SupplierId]);
