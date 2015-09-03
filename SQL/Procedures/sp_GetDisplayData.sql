USE [DisplayMonkey]
GO
if OBJECT_ID('sp_GetDisplayData','P') is null 
	exec ('create proc dbo.sp_GetDisplayData as select 1');
go
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
  2014-11-16 [LTL] - displayId instead of panelId, fixed location
  2015-07-30 [LTL] - supersedes sp_GetIdleInterval and fn_GetDisplayHash, RC13
  2015-08-17 [LTL] - fixed hash, RC14
  2015-09-03 [LTL] - added RecycleTime, RC15
*******************************************************************/
alter procedure dbo.sp_GetDisplayData
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
		inner join FullScreen f with(nolock) on f.CanvasId=d.CanvasId
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
		DisplayId		= DisplayId
	,	[Hash]			= checksum(@v)
	,	IdleInterval	= case when 0 < MaxDuration and MaxDuration < Duration then MaxDuration else Duration end
	,	RecycleTime
	from _f outer apply _m
	;
end
go

