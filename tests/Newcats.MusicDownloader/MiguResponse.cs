namespace Newcats.MusicDownloader
{
    /// <summary>
    /// 专辑封面
    /// </summary>
    public class ImgItem
    {
        /// <summary>
        /// 类型(越大越清晰)
        /// </summary>
        public string imgSizeType { get; set; }

        public int imgSize
        {
            get
            {
                int size = 0;
                if (!string.IsNullOrWhiteSpace(imgSizeType))
                {
                    size = int.Parse(imgSizeType);
                }
                return size;
            }
        }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string img { get; set; }
    }

    /// <summary>
    /// 歌曲文件信息
    /// </summary>
    public class NewRateFormatsItem
    {
        /// <summary>
        /// 音质(ZQ>SQ>HQ>PQ)
        /// </summary>
        public string formatType { get; set; }

        /// <summary>
        /// ftp地址
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string size { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long sizeLong
        {
            get
            {
                long sizeLL = 0;
                if (!string.IsNullOrWhiteSpace(size))
                {
                    sizeLL = long.Parse(size);
                }
                return sizeLL;
            }
        }

        /// <summary>
        /// 文件扩展名(flac/mp3)
        /// </summary>
        public string fileType { get; set; }

        /// <summary>
        /// android文件ftp地址
        /// </summary>
        public string androidUrl { get; set; }

        /// <summary>
        /// android文件大小
        /// </summary>
        public string androidSize { get; set; }

        public long androidSizeLong
        {
            get
            {
                long sizeLL = 0;
                if (!string.IsNullOrWhiteSpace(androidSize))
                {
                    sizeLL = long.Parse(androidSize);
                }
                return sizeLL;
            }
        }
    }

    /// <summary>
    /// 歌曲
    /// </summary>
    public class SongItem
    {
        /// <summary>
        /// 歌曲id
        /// </summary>
        public string songId { get; set; }

        /// <summary>
        /// 歌曲名称
        /// </summary>
        public string songName { get; set; }

        /// <summary>
        /// 歌手
        /// </summary>
        public string singer { get; set; }

        /// <summary>
        /// 专辑id
        /// </summary>
        public string albumId { get; set; }

        /// <summary>
        /// 专辑名称
        /// </summary>
        public string album { get; set; }

        /// <summary>
        /// 专辑封面
        /// </summary>
        public List<ImgItem> albumImgs { get; set; }

        /// <summary>
        /// 歌曲文件信息
        /// </summary>
        public List<NewRateFormatsItem> newRateFormats { get; set; }
    }

    /// <summary>
    /// 资源项
    /// </summary>
    public class ResourceItem
    {
        /// <summary>
        /// 专辑封面
        /// </summary>
        public List<ImgItem> imgItems { get; set; }

        /// <summary>
        /// 歌曲
        /// </summary>
        public List<SongItem> songItems { get; set; }
    }

    /// <summary>
    /// 咪咕api返回消息
    /// </summary>
    public class MiguResponse
    {
        public string code { get; set; }

        public string info { get; set; }

        /// <summary>
        /// 资源
        /// </summary>
        public List<ResourceItem> resource { get; set; }
    }
}