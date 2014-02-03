use [DisplayMonkey]
GO
/*******************************************************************
  2013-11-10 [DPA] - DisplayMonkey object
*******************************************************************/
alter trigger dbo.tr_Canvas_Insert 
	on  Canvas 
	after insert
as
begin
	set nocount on;
	declare @t table (PanelId int,CanvasId int)
	insert Panel (CanvasId, Name, [Top], [Left], Height, Width)
	output inserted.PanelId, inserted.CanvasId into @t
	select i.CanvasId, 'Full-screen', 0, 0, i.Height, i.Width 
	from inserted i left join Panel p on p.CanvasId=i.CanvasId
	where p.CanvasId is null
	;
	insert FullScreen (PanelId,CanvasId) 
	select PanelId,CanvasId from @t
	; 
end
GO
