USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-07 [DPA] - DisplayMonkey object
  2014-02-19 [LTL] - using Sort
  2014-11-07 [LTL] - improvements, blank cycle
  2015-03-30 [LTL] - return entire frame + template
*******************************************************************/
alter procedure dbo.sp_GetNextFrame
	@panelId int
,	@displayId int
,	@lastFrameId int
as begin

	set nocount on;
	declare @now as datetime, @lid int; 
	select 
		@now = getdate()
	;
	
	declare @l table (LocationId int not null, FrameId int not null);
	select @lid=LocationId from Display where DisplayId=@displayId;
	while (not @lid is null) begin
		--print @lid
		insert @l 
			select LocationId, FrameId
			from FrameLocation where LocationId=@lid
			;
		select @lid=AreaId from Location where LocationId=@lid
		;
	end
	;

	with _curr as (
		select isnull(Sort,FrameId) S from Frame where FrameId = @lastFrameId 
	)
	, _next as (
		select 1 S, FrameId, isnull(Sort,FrameId) Sort from Frame 
		inner join _curr on S < isnull(Sort,FrameId) where PanelId = @panelId 
		union all
		select 2 S, FrameId, isnull(Sort,FrameId) Sort from Frame where PanelId = @panelId 
	)
	select top 1 
		t.FrameType
	,	t.Name TemplateName
	,	t.Html
	,	d.FrameId
	,	d.PanelId
	,	d.Duration
	,	d.BeginsOn
	,	d.EndsOn
	,	d.Sort
	,	d.DateCreated
	,	d.CacheMode
	,	d.CacheInterval
	,	d.[Version]
	from _next f 
	inner join Frame d on d.FrameId=f.FrameId
	inner join Template t on t.TemplateId=d.TemplateId
	where
		(d.BeginsOn is null or d.BeginsOn <= @now) 
		and (d.EndsOn is null or @now <= d.EndsOn)
		and (
				not exists(select 1 from FrameLocation where FrameId=f.FrameId)
				or  exists(select 1 from @l where FrameId=f.FrameId)
			)
	order by f.S, f.Sort
	;

end
go

