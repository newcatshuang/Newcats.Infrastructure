namespace Newcats.MusicDownloader
{
    /// <summary>
    /// 歌曲
    /// </summary>
    internal class MusicModel
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 歌曲Id
        /// </summary>
        public string? SongId { get; set; }

        /// <summary>
        /// 歌曲名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 文件扩展名(flac/mp3)
        /// </summary>
        public string? FileExtension { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        public string? FileUrl { get; set; }

        /// <summary>
        /// 歌手
        /// </summary>
        public string? Singer { get; set; }

        /// <summary>
        /// 专辑Id
        /// </summary>
        public string? AlbumId { get; set; }

        /// <summary>
        /// 专辑名称
        /// </summary>
        public string? AlbumName { get; set; }

        /// <summary>
        /// 专辑封面图片地址
        /// </summary>
        public string? AlbumPictureUrl { get; set; }
    }
}