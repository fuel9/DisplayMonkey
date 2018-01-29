USE [DisplayMonkey]
GO
/****** Object:  StoredProcedure [dbo].[sp_RegisterDisplay]    Script Date: 27.01.2018 00:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*******************************************************************
  2013-11-17 [DPA] - DisplayMonkey object
  2015-08-03 [LTL] - defaults for new displays, RC13
  2018-01-27 [   ] - Using DisplayID as identifier instead of host
*******************************************************************/
ALTER procedure [dbo].[sp_RegisterDisplay] (
	@host varchar(100)
,	@name nvarchar(100)
,	@canvasId int
,	@locationId int
,   @displayId int
,	@displayIdReturn int out
)
as begin
set nocount on;

if( isnull(@host,'')='' ) return;
if( isnull(@name,'')='' ) return;
if( isnull(@canvasId,0)=0 ) return;
if( isnull(@locationId,0)=0 ) return;

if not exists(select 1 from Display where DisplayId=@displayId) begin
	insert Display (Name, Host, CanvasId, LocationId)
	select @name, @host, @canvasId, @locationId
	;
	
	set @displayIdReturn = scope_identity()
	;

	-- defaults
	update Display set 
		ErrorLength		= ISNULL((select convert(int,[Value]) from Settings where [Key]='2F43071D-C314-4C78-8438-76B474364258'), ErrorLength)
	,	PollInterval	= ISNULL((select convert(int,[Value]) from Settings where [Key]='7C993AC3-2E15-44DD-84B3-D13935BD1E43'), PollInterval)
	,	ReadyTimeout	= ISNULL((select convert(int,[Value]) from Settings where [Key]='3AC645E8-30B9-4473-A78C-69DBC4BFFAA6'), ReadyTimeout)
	where DisplayId = @displayIdReturn
	;

end else begin
	update Display set Name=@name, CanvasId=@canvasId, LocationId=@locationId, Host=@host
	where DisplayId=@displayId;
	;
	
	set @displayIdReturn = @displayId
	;
end

end
