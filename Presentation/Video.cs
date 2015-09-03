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
using System.Text;
using System.Web.Script.Serialization;

namespace DisplayMonkey
{
	public class Video : Frame
	{
        public bool PlayMuted { get; private set; }
        public bool AutoLoop { get; private set; }
        public string NoVideoSupport { get; private set; }
        public List<VideoAlternative> VideoAlternatives { get; private set; }

        public Video(int frameId)
            : base(frameId)
        {
            _init();
        }

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

        public string Hash
        {
            get
            {
                string hash = null;
                uint crc32 = HttpRuntime.Cache.GetItemCrc32(this.CacheKey);
                hash = (crc32 != 0) ? crc32.ToString() :
                    this.Version.ToString();                // use content version to avoid spinning browser cache

                //System.Diagnostics.Debug.Print(string.Format("???: key={0} hash={1}", this.CacheKey, hash));
                return hash;
            }
        }

        [ScriptIgnore]
        public UInt64 Version { get; private set; }

        [ScriptIgnore]
        public string CacheKey { get; private set; }

        private VideoAlternative()
        {
        }

        public VideoAlternative(Video _video, int _contentId)
        {
            string sql = string.Format(
                "SELECT top 1 c.ContentId, Name, convert(varbinary(256),Data) Chunk, c.Version FROM VideoAlternative a INNER JOIN Content c ON c.ContentId=a.ContentId WHERE a.FrameId={0} and a.ContentId={1};",
                _video.FrameId, _contentId
                );

            using (DataSet ds = DataAccess.RunSql(sql))
            {
                DataRowCollection rows = ds.Tables[0].Rows;
                if (rows.Count > 0)
                {
                    this._initFromRow(rows[0]);
                    this.CacheKey = this.cacheKeyForVideoId(_video.FrameId);
                }
            }
        }

        public string cacheKeyForVideoId(int _frameId)
        {
            return string.Format("video_{0}_{1}_{2}", _frameId, this.Version, this.ContentId);
        }

        private void _initFromRow(DataRow _dr)
        {
            ContentId = _dr.IntOrZero("ContentId");
            Name = _dr.StringOrBlank("Name").Trim();
            Chunk = (byte[])_dr["Chunk"];
            Version = BitConverter.ToUInt64((byte[])_dr["Version"], 0);       // is never a null
        }

        public static List<VideoAlternative> List(int frameId)
        {
            List<VideoAlternative> list = null;

            string sql = string.Format(
                "SELECT c.ContentId, Name, convert(varbinary(256),Data) Chunk, c.Version FROM VideoAlternative a INNER JOIN Content c ON c.ContentId=a.ContentId WHERE a.FrameId={0};",
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
                            VideoAlternative va = new VideoAlternative();
                            va._initFromRow(dr);
                            va.CacheKey = va.cacheKeyForVideoId(frameId);
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