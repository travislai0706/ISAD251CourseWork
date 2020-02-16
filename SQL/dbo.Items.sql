CREATE TABLE [dbo].[Items] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [Name]         VARCHAR (50)    NULL,
    [Slug]         VARCHAR (50)    NULL,
    [Description]  VARCHAR (MAX)   NULL,
    [Price]        NUMERIC (18, 2) NULL,
    [CategoryName] VARCHAR (50)    NULL,
    [CategoryId]   INT             NULL,
    [ImageName ]   VARCHAR (100)   NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

