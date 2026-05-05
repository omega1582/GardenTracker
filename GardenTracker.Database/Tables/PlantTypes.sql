CREATE TABLE [dbo].[PlantTypes]
(
    [Id]   INT            NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(100)  NOT NULL,

    CONSTRAINT [PK_PlantTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE INDEX [IX_PlantTypes_Name] ON [dbo].[PlantTypes] ([Name]);
