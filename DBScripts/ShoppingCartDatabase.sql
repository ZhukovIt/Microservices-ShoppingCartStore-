CREATE DATABASE [ShoppingCart]
GO
USE [ShoppingCart]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCart](
	[ShoppingCartID] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_ShoppingCart] PRIMARY KEY CLUSTERED 
(
	[ShoppingCartID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCartItem](
	[ShoppingCartItemID] [int] IDENTITY(1,1) NOT NULL,
	[ShoppingCartID] [int] NOT NULL,
	[ProductCatalogId] [bigint] NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[ProductDescription] [nvarchar](500) NULL,
	[Amount] [int] NOT NULL,
	[Currency] [nvarchar](5) NOT NULL,
 CONSTRAINT [PK_ShoppingCartItem] PRIMARY KEY CLUSTERED 
(
	[ShoppingCartItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ShoppingCartID_FK] ON [dbo].[ShoppingCartItem]
(
	[ShoppingCartID] ASC
)
GO
ALTER TABLE [dbo].[ShoppingCartItem] WITH CHECK ADD CONSTRAINT [FK_ShoppingCartItem_ShoppingCart] FOREIGN KEY ([ShoppingCartID])
REFERENCES [dbo].[ShoppingCart] ([ShoppingCartID])
GO
CREATE TABLE [dbo].[ShoppingCartEvent](
	[ShoppingCartEventID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[OccuredAt] [datetime] NOT NULL,
	[ShoppingCartItemID] [int] NOT NULL
 CONSTRAINT [PK_ShoppingCartEvents] PRIMARY KEY CLUSTERED 
(
	[ShoppingCartEventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ShoppingCartItemID_FK] ON [dbo].[ShoppingCartEvent]
(
	[ShoppingCartItemID] ASC
)
GO
ALTER TABLE [dbo].[ShoppingCartEvent] WITH CHECK ADD CONSTRAINT [FK_ShoppingCartEvent_ShoppingCartItem] FOREIGN KEY ([ShoppingCartItemID])
REFERENCES [dbo].[ShoppingCartItem] ([ShoppingCartItemID])
GO