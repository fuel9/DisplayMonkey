/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

use [DisplayMonkey]
GO
if object_id('tr_Location','TR') is null exec('
create trigger dbo.tr_Location
on Location after update, delete as select 1
');
go
/*******************************************************************
  2014-11-28 [LTL] - DisplayMonkey object
*******************************************************************/
alter trigger dbo.tr_Location
	on  Location 
	after update, delete
as
begin
	declare @k table (id int,i int);
	declare @i int; set @i=0;
	insert @k (id,i) select LocationId, @i from deleted;
	while (@@rowcount > 0) begin
		set @i=@i+1;
		update c set Name=c.Name
		output inserted.LocationId,@i into @k (id,i)
		from @k p inner join Location c on c.AreaId=p.id
		where i=@i-1
		;
	end
end
GO
