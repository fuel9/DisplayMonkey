use [DisplayMonkey]
GO
/*******************************************************************
  2014-03-17 [DPA] - DisplayMonkey object
*******************************************************************/
alter trigger dbo.tr_FullScreen_Update 
	on  FullScreen 
	after update
as
begin
	set nocount on;
	update c set Name = c.Name
	from Canvas c inner join inserted i on i.CanvasId=c.CanvasId
end
GO
