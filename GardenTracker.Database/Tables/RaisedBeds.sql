CREATE TABLE [dbo].[RaisedBeds]
(
    [Id]                    INT            NOT NULL IDENTITY(1,1),
    [GardenId]              INT            NOT NULL,
    [Name]                  NVARCHAR(100)  NOT NULL,
    [LengthFt]              DECIMAL(6,2)   NOT NULL,
    [WidthFt]               DECIMAL(6,2)   NOT NULL,
    [DepthIn]               DECIMAL(6,2)   NOT NULL,
    [Material]              NVARCHAR(100)  NULL,
    [ExpectedLifespanYears] INT            NOT NULL,
    [InstalledDate]         DATE           NOT NULL,
    [RetiredDate]           DATE           NULL,
    [Notes]                 NVARCHAR(MAX)  NULL,

    CONSTRAINT [PK_RaisedBeds]         PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RaisedBeds_Gardens] FOREIGN KEY ([GardenId]) REFERENCES [dbo].[Gardens] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_RaisedBeds_GardenId] ON [dbo].[RaisedBeds] ([GardenId]);
