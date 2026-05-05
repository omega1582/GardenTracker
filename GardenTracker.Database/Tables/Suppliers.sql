CREATE TABLE [dbo].[Suppliers]
(
    [Id]           INT            NOT NULL IDENTITY(1,1),
    [Name]         NVARCHAR(150)  NOT NULL,
    [SupplierType] INT            NOT NULL,
    [Website]      NVARCHAR(500)  NULL,
    [Notes]        NVARCHAR(MAX)  NULL,

    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([Id] ASC)
);
