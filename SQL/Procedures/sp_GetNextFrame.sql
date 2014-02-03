USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
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
		print @lid
		insert @l 
			select LocationId, FrameId
			from FrameLocation where LocationId=@lid
			;
		select @lid=AreaId from Location where LocationId=@lid
		;
		if (@lid is null) break;
	end
	;

	with _id1 as (
		select 1 S, FrameId, Duration, isnull(BeginsOn,'') BeginsOn, isnull(EndsOn ,'99991231') EndsOn
		from Frame f inner join Panel p on p.PanelId=f.PanelId
		where p.PanelId = @panelId and f.FrameId > @lastFrameId 
		union
		select 2 S, FrameId, Duration, isnull(BeginsOn,'') BeginsOn, isnull(EndsOn ,'99991231') EndsOn
		from Frame f inner join Panel p on p.PanelId=f.PanelId
		where p.PanelId = @panelId 
	)
	select top 1 
		@nextFrameId = f.FrameId
	,	@duration = Duration
	--,	@url=(case when Content_Type = 3 then String1 else null end)
	,	@frameType = t.FrameType
	from _id1 f inner join Frame_Type_View t on t.FrameId=f.FrameId
	where
		@now between BeginsOn and EndsOn
		and (
				not exists(select 1 from FrameLocation where FrameId=f.FrameId)
				or exists(select 1 from @l where FrameId=f.FrameId)
			)
	order by S, f.FrameId

end
go


