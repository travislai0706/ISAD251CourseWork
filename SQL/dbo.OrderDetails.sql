CREATE TABLE [dbo].[OrderDetails] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [OrderId]  INT NULL,
    [UserId]   INT NULL,
    [ItemId]   INT NULL,
    [Quantity] INT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

