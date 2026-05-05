CREATE TABLE [dbo].[Harvests]
(
    [Id]             INT             NOT NULL IDENTITY(1,1),
    [BedId]          INT             NOT NULL,
    [SeasonId]       INT             NOT NULL,
    [PlantVarietyId] INT             NOT NULL,
    [Quantity]       DECIMAL(10,3)   NOT NULL,
    [Unit]           INT             NOT NULL,
    [HarvestDate]    DATE            NOT NULL,
    [Notes]          NVARCHAR(MAX)   NULL,

    CONSTRAINT [PK_Harvests]                    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Harvests_RaisedBeds]         FOREIGN KEY ([BedId])          REFERENCES [dbo].[RaisedBeds]     ([Id]),
    CONSTRAINT [FK_Harvests_Seasons]            FOREIGN KEY ([SeasonId])       REFERENCES [dbo].[Seasons]        ([Id]),
    CONSTRAINT [FK_Harvests_PlantVarieties]     FOREIGN KEY ([PlantVarietyId]) REFERENCES [dbo].[PlantVarieties] ([Id])
);

GO

CREATE INDEX [IX_Harvests_BedId] ON [dbo].[Harvests] ([BedId]);

GO

CREATE INDEX [IX_Harvests_SeasonId] ON [dbo].[Harvests] ([SeasonId]);

GO

CREATE INDEX [IX_Harvests_PlantVarietyId] ON [dbo].[Harvests] ([PlantVarietyId]);
