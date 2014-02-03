USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-17 [DPA] - DisplayMonkey object
*******************************************************************/
alter procedure dbo.sp_RegisterDisplay (
	@host varchar(100)
,	@name nvarchar(100)
,	@canvasId int
,	@locationId int
,	@displayId int out
)
as begin
set nocount on;

if( isnull(@host,'')='' ) return;
if( isnull(@name,'')='' ) return;
if( isnull(@canvasId,0)=0 ) return;
if( isnull(@locationId,0)=0 ) return;

if not exists(select 1 from Display where Host=@host)
	insert Display (Name, Host, CanvasId, LocationId)
	select @name, @host, @canvasId, @locationId
	;
else
	update Display set Name=@name, CanvasId=@canvasId, LocationId=@locationId
	where Host=@host;
	;
	
set @displayId = scope_identity();

end
go

