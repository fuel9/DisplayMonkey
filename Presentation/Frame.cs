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

namespace DisplayMonkey
{
    public class Frame
	{
        public int FrameId { get; protected set; }
        public int PanelId { get; protected set; }
        public int Duration { get; protected set; }
        public int Sort { get; protected set; }
        public string FrameType { get; protected set; }
        public string Template { get; private set; }
        public virtual string Html 
        { 
            get 
            {
                if (_templatePath == null)
                {
                    return string.Format(Resources.ErrorContentTypeNotImplemented, FrameType);
                }
                else
                {
                    string html = "";
                    try
                    {
                        // load template
                        string template = File.ReadAllText(string.Format("{0}{1}.html", _templatePath, Template));

                        // fill template
                        if (FrameId > 0)
                        {
                            html = string.Format(template, FrameId);
                        }
                    }

                    catch (Exception ex)
                    {
                        html = ex.Message;
                    }

                    // return html
                    return html;
                }
            } 
        }

        protected Frame(Frame rhs)
        {
            this.DisplayId = rhs.DisplayId;
            this.Duration = rhs.Duration;
            this.FrameId = rhs.FrameId;
            this.FrameType = rhs.FrameType;
            this.PanelId = rhs.PanelId;
            this.Sort = rhs.Sort;
            this.Template = rhs.Template;
        }

        protected Frame(int frameId, int panelId = 0)
        {
            this.FrameId = frameId;
            this.PanelId = panelId != 0 ? panelId : PanelIdFromFrameId(frameId);
        }

        private Frame()
        {
        }

        private void _init()
        {
            string sql = string.Format("SELECT TOP 1 * FROM Frame WHERE FrameId={0}", FrameId);

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    PanelId = dr.IntOrZero("PanelId");
                    Duration = dr.IntOrZero("Duration");
                    Sort = dr.IntOrZero("Sort");
                    //BeginsOn = dr.IntOrZero("BeginsOn");
                    //EndsOn = dr.IntOrZero("EndsOn");
                    //DateCreated = dr.IntOrZero("DateCreated");
                    Template = "default";
                }
            }
        }

        /*public static string FrameTypeFromFrameId(int frameId)
        {
            string type = "";
            string sql = string.Format("SELECT top 1 FrameType from Frame_Type_View WHERE FrameId={0}", frameId);
            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    type = DataAccess.StringOrBlank(ds.Tables[0].Rows[0][0]);
                }
            }
            return type;
        }*/

		public static int PanelIdFromFrameId(int frameId)
		{
            int panelId = 0;
			string sql = string.Format("SELECT top 1 PanelId from Frame WHERE FrameId={0}", frameId);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables[0].Rows.Count > 0)
				{
					panelId = DataAccess.IntOrZero(ds.Tables[0].Rows[0][0]);
				}
			}
			return panelId;
		}

        public static Frame GetNextFrame(int panelId, int displayId, int previousFrameId)
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
				cmd.Parameters.Add("@nextFrameId", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@duration", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@frameType", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;

                DataAccess.ExecuteNonQuery(cmd);

                nci.FrameId = cmd.Parameters["@nextFrameId"].IntOrZero();
                nci.Duration = cmd.Parameters["@duration"].IntOrZero();
                nci.FrameType = cmd.Parameters["@frameType"].StringOrBlank().ToUpper();
            }

            if (nci.FrameId > 0)
            {
                switch (nci.FrameType)
                {
                    case "CLOCK":
                        nci = new Clock(nci);
                        break;

                    case "HTML":
                        nci = new Html(nci);
                        break;

                    case "MEMO":
                        nci = new Memo(nci);
                        break;

                    case "OUTLOOK":
                        nci = new Outlook(nci);
                        break;

                    case "PICTURE":
                        nci = new Picture(nci);
                        break;

                    case "REPORT":
                        nci = new Report(nci);
                        break;

                    case "VIDEO":
                        nci = new Video(nci);
                        break;

                    case "WEATHER":
                        nci = new Weather(nci);
                        break;

                    case "YOUTUBE":
                        nci = new YouTube(nci);
                        break;

                    case "NEWS":
                    default:
                        break;
                }
            }

            return nci;
		}

		#region Protected Members

        protected int DisplayId = 0;
        protected string _templatePath = null;

		#endregion
	}
}