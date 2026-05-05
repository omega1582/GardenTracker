CREATE TABLE [dbo].[Seasons]
(
    [Id]        INT            NOT NULL IDENTITY(1,1),
    [GardenId]  INT            NOT NULL,
    [Year]      INT            NOT NULL,
    [Notes]     NVARCHAR(MAX)  NULL,

    CONSTRAINT [PK_Seasons]         PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Seasons_Gardens] FOREIGN KEY ([GardenId]) REFERENCES [dbo].[Gardens] ([Id]) ON DELETE CASCADE
);

GO

CREATE UNIQUE INDEX [IX_Seasons_GardenId_Year] ON [dbo].[Seasons] ([GardenId], [Year]);
