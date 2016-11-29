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
using DisplayMonkey.Language;
using System.IO;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using DisplayMonkey.Models;
using System.Threading.Tasks;

namespace DisplayMonkey
{
    public class Frame
	{
        public int FrameId { get; protected set; }
        public int PanelId { get; protected set; }
        public int Duration { get; protected set; }
        public int Sort { get; protected set; }
        public DateTime? BeginsOn { get; private set; }
        public DateTime? EndsOn { get; private set; }
        public DateTime? DateCreated { get; private set; }
        public FrameTypes FrameType { get; protected set; }
        public string TemplateName { get; private set; }
        public string Html { get; private set; }
        public UInt64 Version { get; private set; }
        
        public virtual string Hash 
        {
            // this is used to trick browser built-in caching 
            // for certain HTML attributes, like img->src, iframe->src, etc.
            get
            { 
                return this.Version.ToString(); 
            } 
        }

        [ScriptIgnore]
        public int CacheInterval { get; private set; }

        protected Frame()
        {
        }

        protected Frame(int frameId)
        {
            this.FrameId = frameId;
            _init();
        }

        protected Frame(Frame rhs)
        {
            this.DisplayId = rhs.DisplayId;
            this.FrameType = rhs.FrameType;

            this.FrameId = rhs.FrameId;
            this.PanelId = rhs.PanelId;
            this.Duration = rhs.Duration;
            this.Sort = rhs.Sort;
            this.BeginsOn = rhs.BeginsOn;
            this.EndsOn = rhs.EndsOn;
            this.DateCreated = rhs.DateCreated;
            this.TemplateName = rhs.TemplateName;
            this.Html = rhs.Html;
            this.FrameType = rhs.FrameType;
            this.CacheInterval = rhs.CacheInterval;
            this.Version = rhs.Version;
        }

        private void _initfromRow(SqlDataReader dr)
        {
            FrameId = dr.IntOrZero("FrameId");
            PanelId = dr.IntOrZero("PanelId");
            Duration = dr.IntOrDefault("Duration", 60);
            Sort = dr.IntOrZero("Sort");
            BeginsOn = dr.AsNullable<DateTime>("BeginsOn");
            EndsOn = dr.AsNullable<DateTime>("EndsOn");
            DateCreated = dr.AsNullable<DateTime>("DateCreated");
            TemplateName = dr.StringOrDefault("TemplateName", "default");
            Html = dr.StringOrBlank("Html");
            FrameType = (FrameTypes)dr.IntOrZero("FrameType");
            CacheInterval = dr.IntOrZero("CacheInterval");
            CacheInterval = CacheInterval < 0 ? 0 : CacheInterval;
            Version = BitConverter.ToUInt64(dr.ValueOrNull<byte[]>("Version"), 0);       // is never a null
        }

        private void _init()
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 f.*, t.FrameType, t.Html, t.Name TemplateName FROM Frame f inner join Template t on t.TemplateId=f.TemplateId WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", this.FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    this._initfromRow(dr);
                    return false;
                });
            }
        }

        public static async Task<Frame> GetNextFrameAsync(int panelId, int displayId, int previousFrameId)
		{
            Frame nci = new Frame()
            {
                PanelId = panelId,
                DisplayId = displayId
            };

            using (SqlCommand cmd = new SqlCommand("sp_GetNextFrame"))
            {
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@panelId", SqlDbType.Int).Value = panelId;
                cmd.Parameters.Add("@displayId", SqlDbType.Int).Value = displayId;
                cmd.Parameters.Add("@lastFrameId", SqlDbType.Int).Value = previousFrameId;

                await cmd.ExecuteReaderExtAsync((dr) =>
                {
                    nci._initfromRow(dr);
                    return false;
                });
            }

            if (nci.FrameId > 0)
            {
                switch (nci.FrameType)
                {
                    case FrameTypes.Clock:
                        nci = new Clock(nci);
                        break;

                    case FrameTypes.Html:
                        nci = new Html(nci);
                        break;

                    case FrameTypes.Memo:
                        nci = new Memo(nci);
                        break;

                    //case FrameTypes.News:

                    case FrameTypes.Outlook:
                        nci = new Outlook(nci);
                        break;

                    case FrameTypes.Picture:
                        nci = new Picture(nci);
                        break;

                    case FrameTypes.Powerbi:
                        nci = new Powerbi(nci);
                        break;

                    case FrameTypes.Report:
                        nci = new Report(nci);
                        break;

                    case FrameTypes.Video:
                        nci = new Video(nci);
                        break;

                    case FrameTypes.Weather:
                        nci = new Weather(nci);
                        break;

                    case FrameTypes.YouTube:
                        nci = new YouTube(nci);
                        break;

                    default:
                        break;
                }
            }

            return nci;
		}

		#region Protected Members

        protected int DisplayId = 0;

		#endregion
	}
}