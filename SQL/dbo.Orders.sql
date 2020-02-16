CREATE TABLE [dbo].[Orders] (
    [OrderId]     INT           IDENTITY (1, 1) NOT NULL,
    [UserId]      INT           NULL,
    [CreatedDate] DATETIME2 (7) NULL,
    PRIMARY KEY CLUSTERED ([OrderId] ASC)
);

