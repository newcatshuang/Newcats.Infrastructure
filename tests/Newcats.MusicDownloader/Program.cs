using System.Diagnostics;

namespace Newcats.MusicDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //string defaultSavePath = Directory.GetCurrentDirectory();
            string defaultSavePath = @"C:\Users\Newcats\Music\";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==========================咪咕音乐下载器[Newcats-2023/02/25]==========================");
            Console.WriteLine("1.咪咕音乐官网：https://music.migu.cn/v3");
            Console.WriteLine("2.专辑ID：专辑唯一ID，入参支持多个，多个ID请用英文逗号分割，例如：10086,10010");
            Console.WriteLine("3.专辑链接：咪咕音乐的专辑页链接，入参支持多个，多个链接请用英文逗号分割，例如：https://music.migu.cn/v3/music/digital_album/1139605801,https://music.migu.cn/v3/music/album/63205");
            Console.Write($"4.文件保存路径：下载的音乐文件的保存根目录，子目录以[歌手名-专辑名]自动创建，默认路径为 ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(defaultSavePath);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1.专辑ID：");
            string albumIds = Console.ReadLine();//专辑Id
            Console.WriteLine("2.专辑链接：");
            string albumUrls = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(albumIds) && string.IsNullOrWhiteSpace(albumUrls))
            {
                Console.WriteLine("专辑ID或专辑链接至少要输入一个......");

                Console.WriteLine("1.专辑ID：");
                albumIds = Console.ReadLine();//专辑Id
                Console.WriteLine("2.专辑链接：");
                albumUrls = Console.ReadLine();
            }
            Console.WriteLine("3.文件保存路径：");
            string path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
                path = defaultSavePath;

            if (albumIds == "10086")
            {
                ReportRepeat(path);
            }
            else
            {
                List<string> albumIdList = new();
                if (!string.IsNullOrWhiteSpace(albumIds) && !albumIds.Contains(','))
                {
                    albumIdList.Add(albumIds);
                }
                if (!string.IsNullOrWhiteSpace(albumIds) && albumIds.Contains(','))
                {
                    albumIdList.AddRange(albumIds.Split(',', StringSplitOptions.RemoveEmptyEntries));
                }
                if (!string.IsNullOrWhiteSpace(albumUrls) && !albumUrls.Contains(','))
                {
                    Uri uri = new(albumUrls);
                    albumIdList.Add(uri.Segments.Last());
                }
                if (!string.IsNullOrWhiteSpace(albumUrls) && albumUrls.Contains(','))
                {
                    var urls = albumUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var url in urls)
                    {
                        Uri uri = new(url);
                        albumIdList.Add(uri.Segments.Last());
                    }
                }
                albumIdList = albumIdList.Distinct().ToList();
                int allCount = 0;//总歌曲数
                int allNewCount = 0;//总的新下载数
                int allExistsCount = 0;//总的已存在数
                long totalMilliseconds = 0;//总耗时毫秒数
                foreach (var albumId in albumIdList)
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    var musics = await GetMusicsFromAlbumIdAsync(albumId);
                    if (musics != null && musics.Count > 0)
                    {
                        var (newCount, existsCount) = await DownloadAndSaveMusicAsync(musics, path);
                        allCount += musics.Count;
                        allNewCount += newCount;
                        allExistsCount += existsCount;

                        stopwatch.Stop();
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"[{DateTime.Now}] [{albumId}]下载完成，耗时[{FormatMilliseconds(stopwatch.ElapsedMilliseconds)}] [总列表：{musics.Count}]...[已存在：{existsCount}]...[新下载：{newCount}]");
                        Console.WriteLine();
                    }
                    stopwatch.Stop();
                    totalMilliseconds += stopwatch.ElapsedMilliseconds;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now}] 所有专辑下载完成，耗时[{FormatMilliseconds(totalMilliseconds)}] [歌曲总数：{allCount}]...[已存在：{allExistsCount}]...[新下载：{allNewCount}]");

                ReportRepeat(path);
            }

            Console.ReadLine();
        }

        /// <summary>
        /// 从专辑id解析歌曲信息
        /// </summary>
        /// <param name="albumId"></param>
        /// <returns></returns>
        private static async Task<List<MusicModel>> GetMusicsFromAlbumIdAsync(string albumId)
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
                    Console.ForegroundColor = ConsoleColor.Red;
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
                                SongId = item.songId?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "").Replace("\\", ",").Replace(":", ",").Replace("*", ",").Replace("\"", ",").Replace("<", ",").Replace(">", ",").Replace("|", ","),
                                Name = item.songName?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "").Replace("\\", ",").Replace(":", ",").Replace("*", ",").Replace("\"", ",").Replace("<", ",").Replace(">", ",").Replace("|", ","),
                                Singer = item.singer?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "").Replace("\\", ",").Replace(":", ",").Replace("*", ",").Replace("\"", ",").Replace("<", ",").Replace(">", ",").Replace("|", ","),
                                AlbumId = item.albumId?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "").Replace("\\", ",").Replace(":", ",").Replace("*", ",").Replace("\"", ",").Replace("<", ",").Replace(">", ",").Replace("|", ","),
                                AlbumName = item.album?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "").Replace("\\", ",").Replace(":", ",").Replace("*", ",").Replace("\"", ",").Replace("<", ",").Replace(">", ",").Replace("|", ","),
                                AlbumPictureUrl = item.albumImgs?.OrderByDescending(f => f.imgSize)?.FirstOrDefault().img
                            };

                            var file = item.newRateFormats?.OrderByDescending(f => f.androidSizeLong > 0 ? f.androidSizeLong : f.sizeLong).FirstOrDefault();
                            music.FileExtension = file?.fileType?.Replace(" ", "").Replace("/", "").Replace("?", "").Replace("!", "").Replace("\n", "").Replace("\r", "").Replace("?", "").Replace("!", "");
                            music.FileSize = file?.androidSizeLong > 0 ? file.androidSizeLong : file.sizeLong;
                            string ftpUrl = string.IsNullOrWhiteSpace(file.androidUrl) ? file.url : file.androidUrl;
                            music.FileUrl = $"{httpPrifix}/public{ftpUrl.Split("public").Last()}";

                            musics.Add(music);
                        }
                    }
                }
                if (musics.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[{DateTime.Now}] 解析专辑【{musics.FirstOrDefault()?.AlbumName}】成功，共{musics.Count}首歌曲 O(∩_∩)O");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] 专辑Id={albumId}请求失败 (；′⌒`)");
                }

                return musics;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] 专辑Id={albumId}解析失败 /(ㄒoㄒ)/~~ ____________ {ex.Message}");
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
                tempDownPath = Directory.GetCurrentDirectory();
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
                            Console.WriteLine($"[{DateTime.Now}] {song.Singer}-{song.AlbumName}-{song.Name}  下载完成 φ(゜▽゜*)φ");
                            newCount++;
                        }
                    }
                    else
                    {
                        existsCount++;
                    }

                    try
                    {
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

                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] {song.Singer}-{song.AlbumName}-{song.Name}  下载失败 /(ㄒoㄒ)/~~  ____________ {ex.Message}");
                }
            }

            return (newCount, existsCount);
        }

        /// <summary>
        /// 时间格式化
        /// </summary>
        /// <param name="secs"></param>
        /// <returns></returns>
        private static string FormatMilliseconds(long secs)
        {
            if (secs <= 1000)
                return $"{secs}ms";
            else if (secs > 1000 && secs < 1000 * 60)
                return $"{secs / 1000}s";
            else
                return $"{secs / 1000 / 60}mins";
        }

        /// <summary>
        /// 报告重复的文件信息
        /// </summary>
        /// <param name="basePath"></param>
        private static void ReportRepeat(string basePath)
        {
            var dir = new DirectoryInfo(basePath);
            var allFiles = dir.GetFiles("*.*", SearchOption.AllDirectories);
            if (allFiles != null && allFiles.Any())
            {
                var musics = new List<MusicFileInfo>();
                foreach (var file in allFiles)
                {
                    if (file.Extension.ToLower() != ".jpg" && file.Extension.ToLower() != ".webp" && file.Extension.ToLower() != ".ini")
                    {
                        MusicFileInfo music = new()
                        {
                            FileName = file.Name.Replace(file.Extension, ""),
                            FileSize = file.Length,
                            FullPath = file.FullName.Replace(basePath, ""),
                            FileInfo = file
                        };

                        musics.Add(music);
                    }
                }

                var repeat = musics.GroupBy(f => f.FileName).Where(f => f.ToList().Count() > 1);
                if (repeat != null && repeat.Any())
                {
                    int index = 0;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"==========================[{basePath}]存在{repeat.Count()}组重复名称的文件==========================");
                    foreach (var group in repeat)
                    {
                        index++;
                        Console.WriteLine();
                        foreach (var item in group.OrderByDescending(f => f.FileSize))
                        {
                            Console.WriteLine($"[{DateTime.Now}] {index} ---> {item.FullPath} ---> {item.FileSize}");
                        }
                    }
                }
            }
        }
    }

    public class MusicFileInfo
    {
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string FullPath { get; set; }

        public FileInfo FileInfo { get; set; }
    }
}