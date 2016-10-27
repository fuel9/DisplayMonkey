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
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT TOP 1 * FROM Video WHERE FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", FrameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    PlayMuted = dr.Boolean("PlayMuted");
                    AutoLoop = dr.Boolean("AutoLoop");
                    return false;
                });
            }

            VideoAlternatives = VideoAlternative.List(FrameId);
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
        public string CacheKey 
        {
            get { return string.Format("video_{0}_{1}_{2}", this.FrameId, this.Version, this.ContentId); }
        }

        private VideoAlternative()
        {
        }

        public VideoAlternative(Video _video, int _contentId)
        {
            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText = 
                    "SELECT top 1 a.FrameId, c.ContentId, Name, convert(varbinary(256),Data) Chunk, c.Version FROM VideoAlternative a " +
                    "INNER JOIN Content c ON c.ContentId=a.ContentId WHERE a.FrameId=@frameId and a.ContentId=@contentId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", _video.FrameId);
                cmd.Parameters.AddWithValue("@contentId", _contentId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    _initFromRow(dr);
                    return false;
                });
            }
        }

        public static List<VideoAlternative> List(int frameId)
        {
            List<VideoAlternative> list = new List<VideoAlternative>();

            using (SqlCommand cmd = new SqlCommand()
            {
                CommandType = CommandType.Text,
                CommandText =
                    "SELECT a.FrameId, c.ContentId, Name, convert(varbinary(256),Data) Chunk, c.Version FROM VideoAlternative a " +
                    "INNER JOIN Content c ON c.ContentId=a.ContentId WHERE a.FrameId=@frameId",
            })
            {
                cmd.Parameters.AddWithValue("@frameId", frameId);
                cmd.ExecuteReaderExt((dr) =>
                {
                    VideoAlternative va = new VideoAlternative();
                    va._initFromRow(dr);
                    list.Add(va);
                    return true;
                });
            }

            return list;
        }

        private void _initFromRow(SqlDataReader _dr)
        {
            FrameId = _dr.IntOrZero("FrameId");
            ContentId = _dr.IntOrZero("ContentId");
            Name = _dr.StringOrBlank("Name").Trim();
            Chunk = (byte[])_dr["Chunk"];
            Version = BitConverter.ToUInt64((byte[])_dr["Version"], 0);       // is never a null
        }

        private byte[] Chunk = null;
        private int FrameId = 0;
    }
}