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

-- added OAuth account table
if OBJECT_ID('OauthAccount','U') is null
CREATE TABLE dbo.OauthAccount
(
	AccountId int NOT NULL IDENTITY (1, 1)
		PRIMARY KEY CLUSTERED,
	Provider int NOT NULL,
	Name nvarchar(100) NOT NULL,
	AppId varchar(200) NOT NULL,
	ClientId varchar(4000) NOT NULL,
	ClientSecret varchar(4000) NOT NULL
);
go

-- added provider/account to Weather table
if not exists(select 1 from sys.columns where object_id=object_id('Weather','U') and name='Provider')
	ALTER TABLE dbo.Weather ADD Provider int NULL
;
if not exists(select 1 from sys.columns where object_id=object_id('Weather','U') and name='AccountId')
	ALTER TABLE dbo.Weather ADD AccountId int NULL
;

GO
if not exists(select 1 from sys.objects where type='F' and name='FK_Weather_OauthAccount')
ALTER TABLE dbo.Weather ADD CONSTRAINT
	FK_Weather_OauthAccount FOREIGN KEY
	(
		AccountId
	) REFERENCES dbo.OauthAccount
	(
		AccountId
	) ON UPDATE CASCADE ON DELETE SET NULL
;
go
