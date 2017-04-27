/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Script.Serialization;
using DisplayMonkey.AzureUtil;
using System.Threading.Tasks;

namespace DisplayMonkey
{
    public class Powerbi : Frame
	{
        public string TargetUrl { get; private set; }

        public string Action { get; private set; }

        public async Task<string> GetAccessTokenAsync()
        {
            return await GetAccessTokenAsync(this.AccountId);
        }

        public static async Task<string> GetAccessTokenAsync(int accountId)
        {
            string accessToken = null;

            await DataAccess.ExecuteTransactionAsync(async (cnn, trn) => {
                using (SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = "select top 1 * from AzureAccount with(updlock,rowlock) where AccountId=@accountId",
                    Connection = cnn,
                    Transaction = trn,
                })
                {
                    cmd.Parameters.AddWithValue("@accountId", accountId);

                    DateTime? expiresOn = null;
                    string clientId = null, clientSecret = null, user = null, password = null, tenantId = null;
                    Models.AzureResources resource = Models.AzureResources.AzureResource_PowerBi;
                    await cmd.ExecuteReaderExtAsync((reader) =>
                    {
                        expiresOn = reader.AsNullable<DateTime>("ExpiresOn");
                        accessToken = reader.StringOrBlank("AccessToken").Trim();
                        resource = reader.ValueOrDefault<Models.AzureResources>("Resource", Models.AzureResources.AzureResource_PowerBi);
                        clientId = reader.StringOrBlank("ClientId");
                        clientSecret = reader.StringOrBlank("ClientSecret");
                        user = reader.StringOrBlank("User");
                        password = Encryptor.Current.Decrypt(reader.BytesOrNull("Password"));
                        tenantId = reader.StringOrDefault("TenantId", null);
                        return false;
                    });

                    if (string.IsNullOrWhiteSpace(accessToken) || !expiresOn.HasValue || expiresOn.Value < DateTime.UtcNow)
                    {
                        TokenInfo token = await Token.GetGrantTypePasswordAsync(
                            resource,
                            clientId,
                            clientSecret,
                            user,
                            password,
                            tenantId
                            );
                        using (SqlCommand cmdu = new SqlCommand()
                        {
                            CommandType = CommandType.Text,
                            CommandText = "update AzureAccount set AccessToken=@accessToken, ExpiresOn=@expiresOn where AccountId=@accountId",
                            Connection = cnn,
                            Transaction = trn,
                        })
                        {
                            cmdu.Parameters.AddWithValue("@accountId", accountId);
                            cmdu.Parameters.AddWithValue("@accessToken", token.AccessToken);
                            cmdu.Parameters.AddWithValue("@expiresOn", token.ExpiresOn.AddMinutes(-1)); // allow 1 minute to avoid issues
                            cmdu.ExecuteNonQuery();
                        }
                        accessToken = token.AccessToken;
                    }
                }
            });

            return accessToken;
        }
        
        [ScriptIgnore]
        private int AccountId { get; set; }

        public Powerbi(int frameId)
            : base(frameId)
        {
            _init();
        }

        public Powerbi(Frame frame)
            : base(frame)
		{
            _init();
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Powerbi WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    TargetUrl = dr.StringOrBlank("Url").Trim();
                    Action = (Models.PowerbiTypes)dr.IntOrZero("Type") == Models.PowerbiTypes.PowerbiType_Report ? "loadReport" : "loadTile";
                    AccountId = dr.IntOrZero("AccountId");
                    return false;
                });
            }
        }
    }
}
