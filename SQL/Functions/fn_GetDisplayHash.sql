use DisplayMonkey
go
if OBJECT_ID('fn_GetDisplayHash','FN') is null exec('
	create function dbo.fn_GetDisplayHash() returns int as begin return 0; end
')
go
/*******************************************************************
  2014-10-05 [LTL] - DisplayMonkey object
*******************************************************************/
alter function dbo.fn_GetDisplayHash(@id int) returns int as
begin
	declare @v varbinary(max); set @v = 0x;
	with _c as (
		select c.CanvasId ID, c.Version V from Canvas c with(nolock)
		inner join Display d with(nolock) on d.CanvasId=c.CanvasId 
		where d.DisplayId=@id
	)
	, _l as (
		select l.LocationId ID, l.Version V from Location l with(nolock)
		inner join Display d with(nolock) on d.LocationId=l.LocationId 
		where d.DisplayId=@id
	)
	, _p as (
		select p.PanelId ID, p.Version V from Panel p with(nolock)
		inner join _c on _c.ID=p.CanvasId
	)
	, _h as (
		select 1 O, ID, V from _c
		union all
		select 2 O, ID, V from _l
		union all
		select 3 O, ID, V from _p
	)
	select @v = @v + V from _h order by O, ID
	;
	return checksum(@v)
	;
end
go
