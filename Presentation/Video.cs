using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace DisplayMonkey
{
	public class Video : Frame
	{
        public bool PlayMuted { get; private set; }
        public bool AutoLoop { get; private set; }
        public string NoVideoSupport { get; private set; }
        public List<VideoAlternative> VideoAlternatives { get; private set; }

        public Video(Frame frame)
            : base(frame)
        {
            _init();
        }

        private void _init()
        {
            string sql = string.Format(
                "SELECT TOP 1 * FROM Video WHERE FrameId={0};",
                FrameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    PlayMuted = dr.Boolean("PlayMuted");
                    AutoLoop = dr.Boolean("AutoLoop");
                    VideoAlternatives = VideoAlternative.List(FrameId);
                }
            }

            NoVideoSupport = DisplayMonkey.Language.Resources.BrowserNoVideoSupport;

            _templatePath = HttpContext.Current.Server.MapPath("~/files/frames/video/default.htm");
        }
	}

    public class VideoAlternative
    {
        public string Name { get; private set; }
        public int ContentId { get; private set; }
        public string MimeType
        {
            get
            {
                string mimeType = null;
                try
                {
                    if (null == (mimeType = MimeTypeParser.GetMimeTypeRaw(Chunk)))
                    {
                        if (null == (mimeType = MimeTypeParser.GetMimeTypeFromRegistry(Name)))
                        {
                            if (null == (mimeType = MimeTypeParser.GetMimeTypeFromList(Name)))
                            {
                                mimeType = "application/octet-stream";
                            }
                        }
                    }
                }
                catch
                {
                    mimeType = "application/octet-stream";
                }
                return mimeType;
            }
        }

        private VideoAlternative()
        {
        }

        public static List<VideoAlternative> List(int frameId)
        {
            List<VideoAlternative> list = null;

            string sql = string.Format(
                "SELECT c.ContentId, Name, convert(varbinary(256),Data) Chunk FROM VideoAlternative a INNER JOIN Content c ON c.ContentId=a.ContentId WHERE a.FrameId={0};",
                frameId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                int count = ds.Tables[0].Rows.Count;
                list = new List<VideoAlternative>(count);
                if (count > 0)
                {
                    list.Capacity = count;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (dr["Chunk"] != DBNull.Value)
                        {
                            VideoAlternative va = new VideoAlternative()
                            {
                                ContentId = dr.IntOrZero("ContentId"),
                                Name = dr.StringOrBlank("Name").Trim(),
                                Chunk = (byte[])dr["Chunk"],
                            };
                            list.Add(va);
                        }
                    }
                }
            }

            return list;
        }

        private byte[] Chunk = null;
    }
}