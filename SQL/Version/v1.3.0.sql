/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2016 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

use DisplayMonkey	-- TODO: change if DisplayMonkey database name is different
GO

-- changes for v1.3.0
-- Powerbi

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

if OBJECT_ID('Powerbi','U') is null begin
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
end

GO

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

-- Full Screen

IF  not OBJECT_ID(N'[dbo].[tr_FullScreen_Delete]') is null
	DROP TRIGGER [dbo].[tr_FullScreen_Delete]
;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FullScreen_Canvas]') AND parent_object_id = OBJECT_ID(N'[dbo].[FullScreen]'))
	ALTER TABLE [dbo].[FullScreen] DROP CONSTRAINT [FK_FullScreen_Canvas]
;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FullScreen_Panel]') AND parent_object_id = OBJECT_ID(N'[dbo].[FullScreen]'))
	ALTER TABLE [dbo].[FullScreen] DROP CONSTRAINT [FK_FullScreen_Panel]
;

IF	not object_id('PK_FULL_SCREEN','PK') is null
	ALTER TABLE dbo.FullScreen DROP CONSTRAINT PK_FULL_SCREEN
;

IF	not object_id('PK_FullScreen','PK') is null
	ALTER TABLE dbo.FullScreen DROP CONSTRAINT PK_FullScreen
;

IF  EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FullScreen]') AND name = N'CanvasId')
	ALTER TABLE dbo.FullScreen DROP COLUMN CanvasId
;

ALTER TABLE dbo.FullScreen 
	ADD CONSTRAINT PK_FullScreen PRIMARY KEY CLUSTERED (PanelId)
;

ALTER TABLE dbo.FullScreen 
	ADD CONSTRAINT FK_FullScreen_Panel FOREIGN KEY (PanelId) REFERENCES dbo.Panel (PanelId)
	ON UPDATE  CASCADE 
	ON DELETE  CASCADE 
;

go
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
  2014-11-16 [LTL] - displayId instead of panelId, fixed location
  2015-07-30 [LTL] - supersedes sp_GetIdleInterval and fn_GetDisplayHash, RC13
  2015-08-17 [LTL] - fixed hash, RC14
  2015-09-03 [LTL] - added RecycleTime, RC15
  2016-10-10 [LTL] - handle canvasId in FS, 1.3.0
*******************************************************************/
ALTER procedure [dbo].[sp_GetDisplayData]
	@displayId int
as begin
	set nocount on;

	-- calculate display hash
	declare @v varbinary(max); set @v = 0x;
	with _c as (
		select c.CanvasId ID, c.Version VC, d.Version V
		from Display d with(nolock)
		inner join Canvas c with(nolock) on c.CanvasId=d.CanvasId 
		where d.DisplayId=@displayId
	)
	, _l as (
		select l.LocationId ID, l.Version V from Location l with(nolock)
		inner join Display d with(nolock) on d.LocationId=l.LocationId 
		where d.DisplayId=@displayId
	)
	, _p as (
		select p.PanelId ID, p.Version V from Panel p with(nolock)
		inner join _c on _c.ID=p.CanvasId
	)
	, _h as (
		select 0 O, ID, V from _c
		union all
		select 1 O, ID, VC from _c
		union all
		select 2 O, ID, V from _l
		union all
		select 3 O, ID, V from _p
	)
	select @v = @v + V from _h order by O, ID
	;

	-- claculate FS idle interval
	with _f as (
		select top 1 
			d.DisplayId
		,	d.CanvasId
		,	d.LocationId
		,	d.RecycleTime
		,	f.PanelId
		,	MaxDuration	= isnull(MaxIdleInterval,0)
		from Display d with(nolock)
		inner join Panel p with(nolock) on p.CanvasId=d.CanvasId
		inner join FullScreen f with(nolock) on f.PanelId=p.PanelId
		where d.DisplayId = @displayId
	)
	, _p as (
		select
			p.PanelId
		,	Duration	= sum(isnull(Duration,0))
		from Panel p with(nolock) 
		inner join _f on _f.CanvasId=p.CanvasId
		left join Frame f with(nolock) on f.PanelId=p.PanelId 
			and getdate() between isnull(f.BeginsOn,'') and isnull(f.EndsOn,'99991231')
		where 
			p.PanelId != _f.PanelId	-- exclude FullScreen 
		and (
			-- frame is linked to display location
			exists(select 1 from FrameLocation l with(nolock) where l.FrameId=f.FrameId and l.LocationId=_f.LocationId)
			-- or frame is not linked to any locations
			or not exists(select 1 from FrameLocation l with(nolock) where l.FrameId=f.FrameId)
			)
		group by p.PanelId
	)
	, _m as (
		select
			Duration = max(Duration) from _p
	)

	-- return result
	select 
		DisplayId
	,	[Hash]			= checksum(@v)
	,	IdleInterval	= case when 0 < MaxDuration and MaxDuration < Duration then MaxDuration else Duration end
	,	RecycleTime
	from _f outer apply _m
	;
end
go

declare @html varchar(100); set @html = '<div class="powerbi"><iframe></iframe></div>';
update Template set html=@html
	where FrameType=10 and Name='default'
;
if (@@ROWCOUNT = 0)
	insert Template (Name,Html,FrameType) select 'default',@html,10
;
go

