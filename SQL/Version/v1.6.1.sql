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

-- changes for v1.6.1
-- fixed PowerBI URL length
go

alter table Powerbi alter column Url varchar(4000) null
go
