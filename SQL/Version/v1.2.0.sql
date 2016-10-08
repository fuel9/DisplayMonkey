/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

use DisplayMonkey	-- TODO: change if DisplayMonkey database name is different
GO

-- changes for v1.2.0
update Template set html='
<div class="outlook">
	<div class="progress">
		<img src="files/world.gif" />
	</div>
	<div class="summary">
		<div class="mailbox"></div>
		<div class="current event"></div>
		<div class="current status"></div>
	</div>
	<div class="events">
		<table>
			<tr>
				<th class="label subject"></th>
				<th class="label starts"></th>
				<th class="label ends"></th>
				<th class="label duration"></th>
				<th class="label showAs"></th>
				<th class="label sensitivity"></th>
			</tr>
			<tr class="item">
				<td class="subject"></td>
				<td class="starts"></td>
				<td class="ends"></td>
				<td class="duration"></td>
				<td class="showAs"></td>
				<td class="sensitivity"></td>
			</tr>
		</table>
	</div>
	<div class="actions">
		<div class="title">Quick book</div>
		<div class="message"></div>
		<div class="controls">
			<input type="button" class="book" data-minutes="15" value="0:15" />
			<input type="button" class="book" data-minutes="30" value="0:30" />
			<input type="button" class="book" data-minutes="60" value="1:00" />
			<input type="button" class="book" />
		</div>
	</div>
</div>
'
where FrameType=4 and Name='default'
;

if OBJECT_ID('DF_Outlook_AllowReserve','D') is null
	ALTER TABLE dbo.Outlook ADD AllowReserve bit NOT NULL CONSTRAINT DF_Outlook_AllowReserve DEFAULT 0
;

if OBJECT_ID('DF_Outlook_ShowAsFlags','D') is null
	ALTER TABLE dbo.Outlook ADD ShowAsFlags int NOT NULL CONSTRAINT DF_Outlook_ShowAsFlags DEFAULT -1
;

GO

if OBJECT_ID('AzureAccount','U') is null
CREATE TABLE [dbo].[AzureAccount](
	[AccountId] [int] IDENTITY(1,1) NOT NULL
		CONSTRAINT [PK_PowerbiAccount] PRIMARY KEY CLUSTERED ,
	[Name] [nvarchar](100) NOT NULL,
	[Resource] [int] NOT NULL 
		CONSTRAINT [DF_AzureAccount_Application]  DEFAULT (0) ,
	[ClientId] [varchar](36) NOT NULL,
	[ClientSecret] [varchar](500) NOT NULL,
	[TenantId] [varchar](100) NULL,
	[User] [varchar](100) NOT NULL,
	[Password] [varbinary](400) NULL,
	[AccessToken] [varchar](2000) NULL,
	[ExpiresOn] [datetime] NULL,
	[RefreshToken] [varchar](1000) NULL,
	[IdToken] [varchar](1000) NULL
) ON [PRIMARY]
;

if OBJECT_ID('Powerbi','U') is null
CREATE TABLE [dbo].[Powerbi](
	[FrameId] [int] NOT NULL
		CONSTRAINT [PK_Powerbi] PRIMARY KEY CLUSTERED ,
	[AccountId] [int] NOT NULL,
	[Type] [int] NULL,
	[Name] [nvarchar](100) NULL,
	[Dashboard] [uniqueidentifier] NULL,
	[Tile] [uniqueidentifier] NULL,
	[Report] [uniqueidentifier] NULL,
	[Url] [varchar](200) NULL
) ON [PRIMARY]
;

ALTER TABLE [dbo].[Powerbi] ADD CONSTRAINT [FK_Powerbi_AzureAccount] FOREIGN KEY([AccountId])
	REFERENCES [dbo].[AzureAccount] ([AccountId])
	ON UPDATE CASCADE
	ON DELETE CASCADE
;

ALTER TABLE [dbo].[Powerbi] ADD CONSTRAINT [FK_Powerbi_Frame] FOREIGN KEY([FrameId])
	REFERENCES [dbo].[Frame] ([FrameId])
	ON UPDATE CASCADE
	ON DELETE CASCADE
;

GO

ALTER TABLE dbo.News DROP CONSTRAINT FK_News_Frame
;
ALTER TABLE dbo.News ADD CONSTRAINT FK_News_Frame FOREIGN KEY (FrameId)
	REFERENCES dbo.Frame (FrameId)
	ON UPDATE  CASCADE 
	ON DELETE  CASCADE 
;

IF  not OBJECT_ID('tr_News_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_News_Delete]
;

IF  not OBJECT_ID('tr_Outlook_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Outlook_Delete]
;

IF  not OBJECT_ID('tr_Html_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Html_Delete]
;

IF  not OBJECT_ID('tr_Memo_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Memo_Delete]
;

IF  not OBJECT_ID('tr_Clock_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Clock_Delete]
;

IF  not OBJECT_ID('tr_Weather_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Weather_Delete]
;

IF  not OBJECT_ID('tr_Youtube_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Youtube_Delete]
;

IF  not OBJECT_ID('tr_Report_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Report_Delete]
;

IF  not OBJECT_ID('tr_Picture_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Picture_Delete]
;

IF  not OBJECT_ID('tr_Video_Delete','TR') is null
	DROP TRIGGER [dbo].[tr_Video_Delete]
;

GO

