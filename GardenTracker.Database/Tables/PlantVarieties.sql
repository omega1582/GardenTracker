CREATE TABLE [dbo].[PlantVarieties]
(
    [Id]          INT            NOT NULL IDENTITY(1,1),
    [PlantTypeId] INT            NOT NULL,
    [Name]        NVARCHAR(150)  NOT NULL,
    [Notes]       NVARCHAR(MAX)  NULL,

    CONSTRAINT [PK_PlantVarieties]              PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PlantVarieties_PlantTypes]   FOREIGN KEY ([PlantTypeId]) REFERENCES [dbo].[PlantTypes] ([Id])
);

GO

CREATE UNIQUE INDEX [IX_PlantVarieties_PlantTypeId_Name] ON [dbo].[PlantVarieties] ([PlantTypeId], [Name]);
