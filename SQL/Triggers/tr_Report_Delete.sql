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
/*******************************************************************
  2013-11-10 [DPA] - DisplayMonkey object
*******************************************************************/
alter trigger dbo.tr_Report_Delete 
	on Report 
	after delete
as
begin
	delete f from Frame f inner join deleted d on d.FrameId=f.FrameId
end
GO
