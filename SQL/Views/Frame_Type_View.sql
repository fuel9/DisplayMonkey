USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-14 [DPA] - DisplayMonkey object
*******************************************************************/
alter view dbo.Frame_Type_View as
select
	FrameId
,	FrameType = case 
			when exists(select 1 from Memo d where d.FrameId = f.FrameId) then 'MEMO'
			when exists(select 1 from Picture d where d.FrameId = f.FrameId) then 'PICTURE'
			when exists(select 1 from Weather d where d.FrameId = f.FrameId) then 'WEATHER'
			when exists(select 1 from Report d where d.FrameId = f.FrameId) then 'REPORT'
			when exists(select 1 from Video d where d.FrameId = f.FrameId) then 'VIDEO'
			when exists(select 1 from Clock d where d.FrameId = f.FrameId) then 'CLOCK'
			when exists(select 1 from News d where d.FrameId = f.FrameId) then 'NEWS'
			else 'UNKNOWN' end
from Frame f
go
