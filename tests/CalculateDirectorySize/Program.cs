class Program
{
    static int index = 0;

    static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("==========================文件夹统计[Newcats-2023/12/27]==========================");
        Console.WriteLine();

        Console.Write("请输入要统计的文件夹完整路径：");
        string path = Console.ReadLine();
        Console.Write("请输入要显示的数量：");
        string topString = Console.ReadLine();
        int top = 50;
        if (int.TryParse(topString, out top))
            top = int.Parse(topString);

        var files = GetAllFiles(path);
        var topFiles = files.OrderByDescending(f => f.Size).Take(top);

        List<FileSize> dirSizes = [];
        var allDirs = files.Select(f => f.DirectoryPath).Distinct().OrderBy(f => f);
        foreach (var item in allDirs)
        {
            dirSizes.Add(new FileSize
            {
                DirectoryPath = item,
                Size = files.Where(f => f.DirectoryPath.Contains(item)).Sum(f => f.Size)
            });
        }
        var topDirs = dirSizes.OrderByDescending(f => f.Size).Take(top);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        long totalFileSize = files.Select(f => f.Size).Sum();
        FileSize total = new() { Size = totalFileSize };
        Console.WriteLine($"合计：文件夹总数【{allDirs.Count()}】个，文件总数【{files.Count}】个，文件总大小【{total.SizeString}】");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"文件夹大小前{top}：");
        int indexFile = 1;
        foreach (var dir in topDirs)
        {
            Console.WriteLine(indexFile + ". " + dir.DirectoryPath + "  ---  " + dir.SizeString);
            indexFile++;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"文件大小前{top}：");
        int indexDir = 1;
        foreach (var file in topFiles)
        {
            Console.WriteLine(indexDir + ". " + file.FilePath + "  ---  " + file.SizeString);
            indexDir++;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.ReadLine();
    }

    static List<FileSize> GetAllFiles(string path)
    {
        List<FileSize> fileSizes = [];
        List<string> skipDirs = ["$RECYCLE.BIN", "System Volume Information"];
        foreach (var item in skipDirs)
        {
            if (path.ToUpper().Contains(item.ToUpper()))
                return fileSizes;
        }

        DirectoryInfo directoryInfo = new(path);
        FileInfo[] files = directoryInfo.GetFiles("*", new EnumerationOptions());
        DirectoryInfo[] subDirectories = directoryInfo.GetDirectories("*", new EnumerationOptions());

        foreach (FileInfo file in files)
        {
            fileSizes.Add(new FileSize
            {
                Id = index++,
                FilePath = file.FullName,
                DirectoryPath = file.DirectoryName ?? directoryInfo.FullName,
                Size = file.Length
            });
        }


        foreach (DirectoryInfo subDirectory in subDirectories)
        {
            bool skip = false;
            foreach (var item in skipDirs)
            {
                if (subDirectory.FullName.ToUpper().Contains(item.ToUpper()))
                {
                    skip = true;
                    break;
                }
            }
            if (!skip)
            {
                try
                {
                    var subFiles = GetAllFiles(subDirectory.FullName);
                    if (subFiles != null && subFiles.Count > 0)
                        fileSizes.AddRange(subFiles);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return fileSizes;
    }
}

/// <summary>
/// 文件大小描述
/// </summary>
class FileSize
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 文件全名
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 文件所在目录
    /// </summary>
    public string DirectoryPath { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 人性化大小
    /// </summary>
    public string SizeString
    {
        get
        {
            if (Size == 0)
                return "0 Byte";

            const long oneKilobyte = 1024;//KB
            const long oneMegabyte = oneKilobyte * oneKilobyte;//MB
            const long oneGigabyte = oneMegabyte * oneKilobyte;//GB
            const long oneTerabyte = oneGigabyte * oneKilobyte;//TB

            if (Size < oneKilobyte)
                return Size + " Bytes";
            else if (Size < oneMegabyte)
                return (Size / oneKilobyte).ToString("F2") + " KB";
            else if (Size < oneGigabyte)
                return (Size / oneMegabyte).ToString("F2") + " MB";
            else if (Size < oneTerabyte)
                return (Size / oneGigabyte).ToString("F2") + " GB";
            else
                return (Size / oneTerabyte).ToString("F2") + " TB";
        }
    }
}