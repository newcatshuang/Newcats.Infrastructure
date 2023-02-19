using System.Diagnostics;
using System.Text;
using AngleSharp.Html.Parser;
using Newcats.Utils.Extensions;

namespace MiGuHtmlToSongs
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now}] 开始解析...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            const string TempDownPath = @"C:\\Users\\Newcats\\Music\\TempDown\";
            string path = @"C:\\Users\\Newcats\\Downloads\\歌曲\";

            //string md1 = @"C:\\Users\\Newcats\\Music\\TempDown\\周杰伦\\十一月的萧邦\\一路向北 (电影《头文字Ｄ》插曲).flac";
            //string md2 = @"C:\\Users\\Newcats\\Music\\周杰伦\\十一月的萧邦\\一路向北.flac";

            //Console.WriteLine(GetMD5HashFromFile(md1));
            //Console.WriteLine(GetMD5HashFromFile(md2));


            var list = GetSongsFromFiles(path);

            var (newCount, existsCount) = await DownloadSongs(list, TempDownPath);

            stopwatch.Stop();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now}] 全部下载完成，耗时[{stopwatch.ElapsedMilliseconds / 1000 / 60} mins]  o(*￣▽￣*)ブ   [总列表：{list.Count}].....[已存在：{existsCount}].....[新下载：{newCount}]");
            Console.Read();
        }

        private static List<SongModel> GetSongsFromFiles(string directoryPath)
        {
            DirectoryInfo dir = new(directoryPath);
            FileInfo[] files = dir.GetFiles();

            List<SongModel> songList = new();
            foreach (FileInfo file in files)
            {
                string defaultAlbum = file.Name.Split('-').Last().Split('.').First();
                string defaultArtist = file.Name.Split('-').First();
                using (FileStream fs = new(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    int n = fs.Read(bytes, 0, (int)fs.Length);
                    string htmlText = Encoding.UTF8.GetString(bytes, 0, n);
                    var parser = new HtmlParser();
                    var doc = parser.ParseDocument(htmlText);

                    var songDiv = doc.All.Where(f => f.ClassList.Contains("J_CopySong"));//所有的歌曲div
                    int index = 1;
                    foreach (var song in songDiv)
                    {
                        SongModel ge = new()
                        {
                            Id = Newcats.Utils.Helpers.IdHelper.Create(),
                            Index = index
                        };
                        var s1 = song.GetElementsByClassName("J_SongName");
                        ge.Name = s1.FirstOrDefault().FirstElementChild.TextContent.Trim().Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "");
                        var last = s1.FirstOrDefault().LastElementChild;
                        ge.Quality = last.TextContent.Trim();
                        ge.FileUrl = last.GetAttribute("href").Trim();

                        //ge.Artist = song.GetElementsByClassName("J_SongSingers").FirstOrDefault().TextContent.Trim().Replace("/", "").Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "");
                        ge.Artist = defaultArtist;
                        var dataJson = song.GetElementsByClassName("J-btn-share").FirstOrDefault().GetAttribute("data-share");
                        AlbumDescription album = dataJson.Deserialize<AlbumDescription>();
                        ge.Album = album.album.Trim().Replace("?", "").Replace("!", "");
                        ge.AlbumPictureUrl = album.imgUrl.Trim();

                        if (ge.Album.IsNullOrWhiteSpace())
                            ge.Album = defaultAlbum;
                        if (ge.AlbumPictureUrl.HasValue() && !ge.AlbumPictureUrl.Contains("http:", StringComparison.OrdinalIgnoreCase))
                            ge.AlbumPictureUrl = $"http:{ge.AlbumPictureUrl}";

                        index++;
                        songList.Add(ge);
                    }
                }
            }

            return songList;
        }

        private static async Task<(int, int)> DownloadSongs(List<SongModel> songs, string TempDownPath)
        {
            int newCount = 0;
            int existsCount = 0;
            var baseDir = new DirectoryInfo(TempDownPath);
            if (!baseDir.Exists)
                baseDir.Create();

            foreach (var song in songs)
            {
                try
                {
                    //歌手文件夹
                    var artistDir = new DirectoryInfo(Path.Combine(baseDir.FullName, song.Artist));
                    if (!artistDir.Exists)
                        artistDir.Create();

                    //专辑文件夹
                    var albumDir = new DirectoryInfo(Path.Combine(artistDir.FullName, song.Album));
                    if (!albumDir.Exists)
                        albumDir.Create();

                    //下载歌曲
                    var fileName = Path.Combine(albumDir.FullName, song.Name + "." + song.FileUrl.Split('.').Last());
                    FileInfo fileInfo = new(fileName);
                    if (!fileInfo.Exists)
                    {
                        HttpClient http = new();
                        var fileStream = await http.GetStreamAsync(song.FileUrl);
                        using (var fs = File.Create(fileName))
                        {
                            fileStream.CopyTo(fs);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[{DateTime.Now}] {song.Index}-{song.Artist}-{song.Album}-{song.Name}    下载完成 φ(゜▽゜*)φ");
                            newCount++;
                        }
                    }
                    else
                    {
                        existsCount++;
                    }

                    //下载专辑封面
                    var pic = Path.Combine(albumDir.FullName, song.Album + "." + song.AlbumPictureUrl.Split('.').Last());
                    FileInfo picFile = new(pic);
                    if (!picFile.Exists)
                    {
                        HttpClient http = new();
                        var fileStream = await http.GetStreamAsync(song.AlbumPictureUrl);
                        using (var fs = File.Create(pic))
                        {
                            fileStream.CopyTo(fs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] {song.Index}-{song.Artist}-{song.Album}-{song.Name}    下载失败 /(ㄒoㄒ)/~~  ____________ {ex.Message}");
                    //throw;
                }
            }

            return (newCount, existsCount);
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}