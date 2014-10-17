USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
  2014-11-16 [LTL] - displayId instead of panelId, fixed location
*******************************************************************/
alter procedure dbo.sp_GetIdleInterval (
	@displayId int
,	@idleInterval int out
)
as begin
set @idleInterval = 0;
with _f as (
	select top 1 
		d.CanvasId
	,	d.LocationId
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
	left join Weather x1 with(nolock) on x1.FrameId=f.FrameId
	left join Clock x2 with(nolock) on x2.FrameId=f.FrameId
	left join News x3 with(nolock) on x3.FrameId=f.FrameId
	where 
		p.PanelId != _f.PanelId	-- exclude FullScreen 
	and x1.FrameId is null		-- exclude WEATHER
	and x2.FrameId is null		-- exclude CLOCK
	and x3.FrameId is null		-- exclude NEWS
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
select @idleInterval = case when 0 < MaxDuration and MaxDuration < Duration then MaxDuration else Duration end
from _f left join _m on 1=1
;
end
go

