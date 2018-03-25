/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2018 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

use DisplayMonkey	-- TODO: change if DisplayMonkey database name is different
GO

-- changes for v1.6.0
-- added display auto load mode
go

/*******************************************************************
  2013-11-17 [DPA] - DisplayMonkey object
  2015-08-03 [LTL] - defaults for new displays, RC13
  2018-01-27 [JBI] - Using DisplayID as identifier instead of host, #37
  2018-02-10 [DPA] - added auto load mode parameter, #37
*******************************************************************/
alter procedure [dbo].[sp_RegisterDisplay] (
	@host varchar(100)
,	@name nvarchar(100)
,	@canvasId int
,	@locationId int
,	@displayId int out
)
as begin
	set nocount on;

	-- TODO: error handling
	declare @autoLoadMode int;		
	select
		@autoLoadMode = 0			-- by host/IP
	,	@name = isnull(@name,'')
	,	@displayId = ISNULL(@displayId, 0)
	,	@canvasId = isnull(@canvasId,0)
	,	@locationId = isnull(@locationId,0)
	;
	
	if( @name = '' ) return;
	if( @canvasId = 0 or not exists(select 1 from Canvas where CanvasId = @canvasId) ) return;
	if( @locationId = 0 or not exists(select 1 from Location where LocationId = @locationId) ) return;

	select @autoLoadMode = convert(int,[Value]) from Settings where [Key]='AE1B2F10-9EC3-4429-97B5-C12D64575C41'
	;

	if (@autoLoadMode = 0)
		select top 1 @displayId = DisplayId from Display where Host=@host order by Name;
	else if (@autoLoadMode = 1)
		set @displayId = isnull((select DisplayId from Display where DisplayId=@displayId),0);
	;

	if (@displayId = 0) begin
		insert Display (Name, Host, CanvasId, LocationId)
			select @name, @host, @canvasId, @locationId
		;
		
		set @displayId = scope_identity()
		;

		-- defaults
		update Display set 
			ErrorLength		= ISNULL((select convert(int,[Value]) from Settings where [Key]='2F43071D-C314-4C78-8438-76B474364258'), ErrorLength)
		,	PollInterval	= ISNULL((select convert(int,[Value]) from Settings where [Key]='7C993AC3-2E15-44DD-84B3-D13935BD1E43'), PollInterval)
		,	ReadyTimeout	= ISNULL((select convert(int,[Value]) from Settings where [Key]='3AC645E8-30B9-4473-A78C-69DBC4BFFAA6'), ReadyTimeout)
		where DisplayId = @displayId
		;

	end else begin
	
		update Display set Name=@name, CanvasId=@canvasId, LocationId=@locationId
		where DisplayId = @displayId
		;
		
	end

end
go
