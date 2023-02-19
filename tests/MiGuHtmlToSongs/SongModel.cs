namespace MiGuHtmlToSongs
{
    /// <summary>
    /// 歌曲模型
    /// </summary>
    public class SongModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 艺术家
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// 专辑名
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// 专辑照片
        /// </summary>
        public string AlbumPictureUrl { get; set; }

        /// <summary>
        /// 歌曲名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 音质
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        public string FileUrl { get; set; }
    }

    public class AlbumDescription
    {
        public string imgUrl { get; set; }

        public string album { get; set; }
    }
}