USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
*******************************************************************/
alter procedure dbo.sp_GetIdleInterval (
	@panelId int
,	@idleInterval int out
)
as begin
set @idleInterval = 0;
with _s as (
	select top 1 
		CanvasId
	,	MaxDuration	= isnull(MaxIdleInterval,0)
	from FullScreen where PanelId=@panelId
)
, _p as (
	select
		f.PanelId
	,	Duration	= sum(isnull(Duration,0))
	from Panel p 
	inner join FullScreen _s on _s.CanvasId=p.CanvasId
	left join Frame f on f.PanelId=p.PanelId
	left join Weather x1 on x1.FrameId=f.FrameId
	left join Clock x2 on x2.FrameId=f.FrameId
	left join News x3 on x3.FrameId=f.FrameId
	where p.PanelId!=@panelId	-- exclude FullScreen 
	-- comment below the wanted ones
	and x1.FrameId is null	-- exclude WEATHER
	and x2.FrameId is null	-- exclude CLOCK
	and x3.FrameId is null	-- exclude NEWS
	group by f.PanelId
)
, _m as (
	select
		Duration = max(Duration) from _p
)
select @idleInterval = case when 0<MaxDuration and MaxDuration<Duration then MaxDuration else Duration end
from _s left join _m on 1=1
;
end
go

