CREATE TABLE [dbo].[Categories] (
    [Id]      INT          IDENTITY (1, 1) NOT NULL,
    [Name]    VARCHAR (50) NULL,
    [Slug]    VARCHAR (50) NULL,
    [Sorting] INT          NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

