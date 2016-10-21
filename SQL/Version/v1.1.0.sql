/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2016 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

use DisplayMonkey	-- TODO: change if DisplayMonkey database name is different
GO

-- changes for v1.1.0
if not exists(select 1 from sys.columns where object_id=object_id('Outlook','U') and name='Privacy') begin
	alter table dbo.Outlook add
		Privacy int NOT NULL constraint DF_Outlook_Privacy default (0)
end
GO
