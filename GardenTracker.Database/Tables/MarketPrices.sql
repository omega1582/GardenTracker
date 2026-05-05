CREATE TABLE [dbo].[MarketPrices]
(
    [Id]             INT            NOT NULL IDENTITY(1,1),
    [SeasonId]       INT            NOT NULL,
    [PlantTypeId]    INT            NOT NULL,
    [PlantVarietyId] INT            NULL,
    [PricePerUnit]   DECIMAL(10,2)  NOT NULL,
    [Unit]           INT            NOT NULL,
    [RecordedDate]   DATE           NOT NULL,

    CONSTRAINT [PK_MarketPrices]                    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MarketPrices_Seasons]            FOREIGN KEY ([SeasonId])       REFERENCES [dbo].[Seasons]        ([Id]),
    CONSTRAINT [FK_MarketPrices_PlantTypes]         FOREIGN KEY ([PlantTypeId])    REFERENCES [dbo].[PlantTypes]     ([Id]),
    CONSTRAINT [FK_MarketPrices_PlantVarieties]     FOREIGN KEY ([PlantVarietyId]) REFERENCES [dbo].[PlantVarieties] ([Id]) ON DELETE SET NULL
);

GO

CREATE INDEX [IX_MarketPrices_SeasonId] ON [dbo].[MarketPrices] ([SeasonId]);

GO

CREATE INDEX [IX_MarketPrices_PlantTypeId] ON [dbo].[MarketPrices] ([PlantTypeId]);

GO

CREATE INDEX [IX_MarketPrices_PlantVarietyId] ON [dbo].[MarketPrices] ([PlantVarietyId]);
