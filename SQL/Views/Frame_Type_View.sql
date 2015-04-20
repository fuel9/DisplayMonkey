USE [DisplayMonkey]
GO
/*******************************************************************
  2013-11-14 [DPA] - DisplayMonkey object
  2014-10-09 [LTL] - added HTML
  2014-10-14 [LTL] - added YOUTUBE
  2015-03-31 [LTL] - deprecated, will be removed in future versions
*******************************************************************/
ALTER view [dbo].[Frame_Type_View] as
select
	FrameId
,	FrameType = case 
			when exists(select 1 from Clock d where d.FrameId = f.FrameId) then 'CLOCK'
			when exists(select 1 from Html d where d.FrameId = f.FrameId) then 'HTML'
			when exists(select 1 from Memo d where d.FrameId = f.FrameId) then 'MEMO'
			when exists(select 1 from News d where d.FrameId = f.FrameId) then 'NEWS'
			when exists(select 1 from Picture d where d.FrameId = f.FrameId) then 'PICTURE'
			when exists(select 1 from Report d where d.FrameId = f.FrameId) then 'REPORT'
			when exists(select 1 from Video d where d.FrameId = f.FrameId) then 'VIDEO'
			when exists(select 1 from Weather d where d.FrameId = f.FrameId) then 'WEATHER'
			when exists(select 1 from Youtube d where d.FrameId = f.FrameId) then 'YOUTUBE'
			else 'UNKNOWN' end
from Frame f
go
