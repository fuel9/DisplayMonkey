use [DisplayMonkey]
GO
/*******************************************************************
  2014-03-17 [DPA] - DisplayMonkey object
*******************************************************************/
alter trigger dbo.tr_Panel_Update 
	on  Panel 
	after insert, update, delete
as
begin
	set nocount on;
	update c set Name = c.Name
	from Canvas c inner join inserted i on i.CanvasId=c.CanvasId
	update c set Name = c.Name
	from Canvas c inner join deleted i on i.CanvasId=c.CanvasId
end
GO
