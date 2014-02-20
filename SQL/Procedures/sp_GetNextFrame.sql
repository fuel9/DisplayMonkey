USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
  2014-02-19 [DPA] - using Sort
*******************************************************************/
alter procedure dbo.sp_GetNextFrame
	@panelId int
,	@displayId int
,	@lastFrameId int
--,	@featureID int
,	@nextFrameId int OUT
,	@duration int OUT
,	@frameType varchar(20) OUT
as begin

	set nocount on;
	declare @now as datetime, @lid int; set @now = getdate();

	declare @l table (LocationId int not null, FrameId int not null);
	select @lid=LocationId from Display where DisplayId=@displayId;
	while(1=1) begin
		--print @lid
		insert @l 
			select LocationId, FrameId
			from FrameLocation where LocationId=@lid
			;
		select @lid=AreaId from Location where LocationId=@lid
		;
		if (@lid is null) break;
	end
	;

	with _curr as (
		select isnull(Sort,FrameId) S from Frame 
		where PanelId = @panelId and FrameId = @lastFrameId 
	)
	, _next as (
		select 1 S, FrameId, isnull(Sort,FrameId) Sort from Frame 
		inner join _curr on S < isnull(Sort,FrameId) where PanelId = @panelId 
		union all
		select 2 S, FrameId, isnull(Sort,FrameId) Sort from Frame where PanelId = @panelId 
	)
	select top 1 
		@nextFrameId = f.FrameId
	,	@duration = d.Duration
	,	@frameType = t.FrameType
	from _next f 
	inner join Frame_Type_View t on t.FrameId=f.FrameId
	inner join Frame d on d.FrameId=f.FrameId
	where
		(d.BeginsOn is null or d.BeginsOn <= @now) 
		and (d.EndsOn is null or @now <= d.EndsOn)
		and (
				not exists(select 1 from FrameLocation where FrameId=f.FrameId)
				or  exists(select 1 from @l where FrameId=f.FrameId)
			)
	order by f.S, f.Sort

end
go


