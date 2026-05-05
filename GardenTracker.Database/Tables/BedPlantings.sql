CREATE TABLE [dbo].[BedPlantings]
(
    [Id]              INT            NOT NULL IDENTITY(1,1),
    [BedId]           INT            NOT NULL,
    [SeasonId]        INT            NOT NULL,
    [PlantVarietyId]  INT            NOT NULL,
    [SupplierId]      INT            NULL,
    [StartMethod]     INT            NOT NULL,
    [Quantity]        INT            NOT NULL,
    [TotalCost]       DECIMAL(10,2)  NOT NULL,
    [SourceHarvestId] INT            NULL,
    [Notes]           NVARCHAR(MAX)  NULL,

    CONSTRAINT [PK_BedPlantings]                    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BedPlantings_RaisedBeds]         FOREIGN KEY ([BedId])           REFERENCES [dbo].[RaisedBeds]     ([Id]),
    CONSTRAINT [FK_BedPlantings_Seasons]            FOREIGN KEY ([SeasonId])        REFERENCES [dbo].[Seasons]        ([Id]),
    CONSTRAINT [FK_BedPlantings_PlantVarieties]     FOREIGN KEY ([PlantVarietyId])  REFERENCES [dbo].[PlantVarieties] ([Id]),
    CONSTRAINT [FK_BedPlantings_Suppliers]          FOREIGN KEY ([SupplierId])      REFERENCES [dbo].[Suppliers]      ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_BedPlantings_Harvests]           FOREIGN KEY ([SourceHarvestId]) REFERENCES [dbo].[Harvests]       ([Id]) ON DELETE SET NULL
);

GO

CREATE INDEX [IX_BedPlantings_BedId] ON [dbo].[BedPlantings] ([BedId]);

GO

CREATE INDEX [IX_BedPlantings_SeasonId] ON [dbo].[BedPlantings] ([SeasonId]);

GO

CREATE INDEX [IX_BedPlantings_PlantVarietyId] ON [dbo].[BedPlantings] ([PlantVarietyId]);

GO

CREATE INDEX [IX_BedPlantings_SupplierId] ON [dbo].[BedPlantings] ([SupplierId]);

GO

CREATE INDEX [IX_BedPlantings_SourceHarvestId] ON [dbo].[BedPlantings] ([SourceHarvestId]);
