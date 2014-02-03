use [DisplayMonkey]
GO
/*******************************************************************
  2013-11-10 [DPA] - DisplayMonkey object
*******************************************************************/
alter trigger dbo.tr_FullScreen_Delete 
	on  FullScreen 
	after delete
as
begin
	set nocount on;
	if exists(
		select 1 from deleted d inner join Canvas c on c.CanvasId=d.CanvasId 
	) begin
		raiserror('A canvas requires one and only full screen panel', 16, 1);
		rollback work;
		return;
	end
end
GO
