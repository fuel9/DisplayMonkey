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

-- changes for v1.5.2
-- added booking subject

ALTER TABLE dbo.Outlook ADD
	BookingSubject nvarchar(100) NULL
GO
