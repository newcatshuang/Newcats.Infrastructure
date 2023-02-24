using System.Reflection;

namespace Newcats.MusicDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string sss = Assembly.GetExecutingAssembly().Location;
            Console.WriteLine("Hello, World!");
            string albumId = Console.ReadLine();


        }

        /// <summary>
        /// 从专辑id解析歌曲信息
        /// </summary>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public static async Task<List<MusicModel>> GetMusicsFromAlbumIdAsync(string albumId)
        {
            try
            {
                List<MusicModel> musics = new();
                const string httpPrifix = "https://freetyst.nf.migu.cn";//固定前缀，替换ftp地址用的
                string fullUrl = $"https://app.c.nf.migu.cn/MIGUM2.0/v1.0/content/resourceinfo.do?needSimple=01&resourceId={albumId}&resourceType=2003";

                HttpClient client = new();
                HttpRequestMessage httpRequest = new(HttpMethod.Get, fullUrl);
                httpRequest.Headers.TryAddWithoutValidation("User-agent", @"Mozilla/5.0 (iPhone; CPU iPhone OS 12_4_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 MicroMessenger/7.0.11(0x17000b21) NetType/4G Language/zh_CN");
                httpRequest.Headers.TryAddWithoutValidation("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                httpRequest.Headers.TryAddWithoutValidation("Host", @"app.c.nf.migu.cn");
                httpRequest.Headers.TryAddWithoutValidation("Content-Type", @"application/json;charset=utf-8");

                var response = await client.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"专辑Id={albumId}请求失败(；′⌒`)");
                }

                var result = await response.Content.ReadAsStringAsync();


                var miguModel = System.Text.Json.JsonSerializer.Deserialize<MiguResponse>(result);
                if (miguModel != null && miguModel.resource != null && miguModel.resource.Any())
                {
                    var resource = miguModel.resource.FirstOrDefault();
                    if (resource != null && resource.songItems != null && resource.songItems.Any())
                    {
                        foreach (var item in resource.songItems)
                        {
                            MusicModel music = new()
                            {
                                SongId = item.songId?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", ""),
                                Name = item.songName?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", ""),
                                Singer = item.singer?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", ""),
                                AlbumId = item.albumId?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", ""),
                                AlbumName = item.album?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", ""),
                                AlbumPictureUrl = item.albumImgs?.OrderByDescending(f => f.imgSize)?.FirstOrDefault().img
                            };

                            var file = item.newRateFormats?.OrderByDescending(f => f.androidSizeLong > 0 ? f.androidSizeLong : f.sizeLong).FirstOrDefault();
                            music.FileExtension = file?.fileType?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "");
                            music.FileSize = file?.androidSizeLong > 0 ? file.androidSizeLong : file.sizeLong;
                            string ftpUrl = string.IsNullOrWhiteSpace(file.androidUrl) ? file.url : file.androidUrl;
                            music.FileUrl = $"{httpPrifix}{ftpUrl.Split("public").Last()}";

                            musics.Add(music);
                        }
                    }
                }
                if (musics.Count > 0)
                    Console.WriteLine($"[{DateTime.Now}] 解析专辑【{musics.FirstOrDefault()?.AlbumName}】成功，共{musics.Count}首歌曲 O(∩_∩)O");
                else
                    Console.WriteLine($"[{DateTime.Now}] 专辑Id={albumId}请求失败 (；′⌒`)");

                return musics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] 专辑Id={albumId}解析失败 /(ㄒoㄒ)/~~");
                return null;
            }
        }

        /// <summary>
        /// 下载并保存歌曲到指定文件夹
        /// </summary>
        /// <param name="songs"></param>
        /// <param name="tempDownPath"></param>
        /// <returns></returns>
        private static async Task<(int, int)> DownloadAndSaveMusicAsync(List<MusicModel> songs, string tempDownPath)
        {
            if (string.IsNullOrWhiteSpace(tempDownPath))
                tempDownPath = Assembly.GetExecutingAssembly().Location;
            int newCount = 0;
            int existsCount = 0;
            var baseDir = new DirectoryInfo(tempDownPath);
            if (!baseDir.Exists)
                baseDir.Create();

            foreach (var song in songs)
            {
                try
                {
                    //歌手文件夹
                    var artistDir = new DirectoryInfo(Path.Combine(baseDir.FullName, song.Singer));
                    if (!artistDir.Exists)
                        artistDir.Create();

                    //专辑文件夹
                    var albumDir = new DirectoryInfo(Path.Combine(artistDir.FullName, song.AlbumName));
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
                            Console.WriteLine($"[{DateTime.Now}] {song.Singer}-{song.AlbumName}-{song.Name}    下载完成 φ(゜▽゜*)φ");
                            newCount++;
                        }
                    }
                    else
                    {
                        existsCount++;
                    }

                    //下载专辑封面
                    var pic = Path.Combine(albumDir.FullName, song.AlbumName + "." + song.AlbumPictureUrl.Split('.').Last());
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
                    Console.WriteLine($"[{DateTime.Now}] {song.Singer}-{song.AlbumName}-{song.Name}    下载失败 /(ㄒoㄒ)/~~  ____________ {ex.Message}");
                }
            }

            return (newCount, existsCount);
        }
    }
}