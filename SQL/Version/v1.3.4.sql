/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2017 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

use DisplayMonkey	-- TODO: change if DisplayMonkey database name is different
GO

-- changes for v1.3.4
-- increased token column sizes

ALTER TABLE dbo.AzureAccount
	alter column AccessToken varchar(8000) NULL
;
ALTER TABLE dbo.AzureAccount
	alter column RefreshToken varchar(8000) NULL
;
ALTER TABLE dbo.AzureAccount
	alter column IdToken varchar(8000) NULL
GO
