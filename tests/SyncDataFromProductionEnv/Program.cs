using SyncDataFromProductionEnv.Services;

// 初始化服务
var databaseService = new DatabaseService();
var syncService = new SyncService();

// 主程序循环
while (true)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==========================生产数据库同步到脱敏环境(Newcats 2025/4/29)==========================\n");
    Console.ResetColor();

    // 加载并显示所有配置
    var definitions = await databaseService.GetAllSyncDefinitionsAsync();

    if (definitions.Any())
    {
        // 按ID正序排序
        var sortedDefinitions = definitions.OrderBy(d => d.Id);

        // 显示表头
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{"ID",-6}{"Key",-20}{"表数量",-8}{"表名列表",-50}{"创建时间",-20}");
        Console.WriteLine(new string('-', Console.WindowWidth - 1));
        Console.ResetColor();

        // 显示数据行
        foreach (var def in sortedDefinitions)
        {
            var displayKey = def.Key.Length <= 17 ? def.Key : def.Key.Substring(0, 17) + "...";
            var displayTables = def.Tables.Length <= 3
                ? string.Join(", ", def.Tables)
                : string.Join(", ", def.Tables.Take(3)) + "...";

            if (displayTables.Length > 47)
                displayTables = displayTables.Substring(0, 47) + "...";

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{def.Id,-6}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{displayKey,-20}");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"{def.Tables.Length,-8}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{displayTables,-50}");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{(def.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"),-20}");
            Console.ResetColor();
        }
        Console.WriteLine();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("暂无同步配置数据\n");
        Console.ResetColor();
    }

    // 询问用户选择
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("请选择操作：");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("1. 使用现有配置同步");
    Console.WriteLine("2. 新增配置并同步");
    Console.WriteLine("3. 查询配置详细信息");
    Console.WriteLine("4. 修改配置Key");
    Console.WriteLine("5. 删除配置");
    Console.WriteLine("Q. 退出程序");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("请输入选项: ");
    Console.ForegroundColor = ConsoleColor.Green;

    var choice = Console.ReadLine()?.ToUpper();
    Console.ResetColor();

    // 处理退出选项
    if (choice == "Q" || choice == "EXIT")
    {
        break;
    }

    // 处理使用现有配置同步
    if (choice == "1")
    {
        if (!definitions.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n当前没有可用的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n请输入要同步的配置ID（多个ID用英文逗号分隔）: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var ids = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();
        Console.ResetColor();

        if (ids == null || !ids.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("未输入有效的ID");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        var keys = definitions.Where(d => ids.Contains(d.Id)).Select(d => d.Key).ToList();
        if (!keys.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("未找到对应的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        // 创建新的CancellationTokenSource
        using var syncCts = new CancellationTokenSource();

        // 启动读秒任务
        var tickerTask = ShowTickerAsync(syncCts.Token);
        var (success, message, duration) = await syncService.SyncDataAsync(keys);
        // 停止读秒
        syncCts.Cancel();
        await tickerTask;

        // 清理当前行
        Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");

        // 显示同步结果
        Console.Write("\n同步");
        Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(success ? "成功" : "失败");
        Console.ResetColor();
        Console.Write(": ");
        Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"耗时: {FormatDuration(duration)}");
        Console.ResetColor();
    }
    // 处理新增配置并同步
    else if (choice == "2")
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n请输入要同步的表名（多个表名用英文逗号分隔）: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var tables = Console.ReadLine()?.Trim();
        Console.ResetColor();

        if (string.IsNullOrEmpty(tables))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("未输入有效的表名");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n是否要手动指定Key？(Y/N): ");
        Console.ForegroundColor = ConsoleColor.Green;
        var specifyKey = Console.ReadLine()?.Trim().ToUpper();
        Console.ResetColor();

        string? customKey = null;
        if (specifyKey == "Y")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("请输入Key: ");
            Console.ForegroundColor = ConsoleColor.Green;
            customKey = Console.ReadLine()?.Trim();
            Console.ResetColor();

            if (string.IsNullOrEmpty(customKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Key不能为空，将使用自动生成的Key");
                Console.ResetColor();
                customKey = null;
            }
        }

        try
        {
            var key = await databaseService.AddSyncDefinitionAsync(tables, customKey);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"已创建新配置，Key: {key}");
            Console.ResetColor();

            // 创建新的CancellationTokenSource
            using var newSyncCts = new CancellationTokenSource();

            // 启动读秒任务
            var tickerTask = ShowTickerAsync(newSyncCts.Token);
            var (success, message, duration) = await syncService.SyncDataAsync(new List<string> { key });
            // 停止读秒
            newSyncCts.Cancel();
            await tickerTask;

            // 清理当前行
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");

            // 显示同步结果
            Console.Write("\n同步");
            Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write(success ? "成功" : "失败");
            Console.ResetColor();
            Console.Write(": ");
            Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"耗时: {FormatDuration(duration)}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n创建失败: {ex.Message}");
            Console.ResetColor();
        }
    }
    // 处理查询配置详细信息
    else if (choice == "3")
    {
        if (!definitions.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n当前没有可用的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n请输入要查询的配置ID: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var input = Console.ReadLine();
        Console.ResetColor();

        if (!int.TryParse(input, out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("无效的ID格式");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        var config = definitions.FirstOrDefault(d => d.Id == id);
        if (config == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("未找到对应的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n配置详细信息：");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("ID: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(config.Id);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Key: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(config.Key);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("创建时间: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(config.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("更新时间: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(config.UpdatedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("表数量: ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(config.Tables.Length);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("表名列表:");
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var table in config.Tables)
        {
            Console.WriteLine($"  - {table}");
        }
        Console.ResetColor();
    }
    // 处理修改配置Key
    else if (choice == "4")
    {
        if (!definitions.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n当前没有可用的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n请输入要修改的配置ID: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var input = Console.ReadLine();
        Console.ResetColor();

        if (!int.TryParse(input, out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("无效的ID格式");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        var config = definitions.FirstOrDefault(d => d.Id == id);
        if (config == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("未找到对应的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("\n当前Key: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(config.Key);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n请输入新的Key: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var newKey = Console.ReadLine()?.Trim();
        Console.ResetColor();

        if (string.IsNullOrEmpty(newKey))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("新Key不能为空");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        try
        {
            await databaseService.UpdateSyncDefinitionKeyAsync(id, newKey);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n配置Key修改成功！");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n修改失败: {ex.Message}");
            Console.ResetColor();
        }
    }
    // 处理删除配置
    else if (choice == "5")
    {
        if (!definitions.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n当前没有可用的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n请输入要删除的配置ID: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var input = Console.ReadLine();
        Console.ResetColor();

        if (!int.TryParse(input, out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("无效的ID格式");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        var config = definitions.FirstOrDefault(d => d.Id == id);
        if (config == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("未找到对应的配置");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        // 显示配置信息并确认
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n要删除的配置信息：");
        Console.Write("ID: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(config.Id);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Key: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(config.Key);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("表数量: ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(config.Tables.Length);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\n确认要删除这个配置吗？(Y/N): ");
        Console.ForegroundColor = ConsoleColor.Green;
        var confirm = Console.ReadLine()?.Trim().ToUpper();
        Console.ResetColor();

        if (confirm != "Y")
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n已取消删除操作");
            Console.ResetColor();
            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
            continue;
        }

        try
        {
            await databaseService.DeleteSyncDefinitionAsync(id);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n配置已成功删除！");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n删除失败: {ex.Message}");
            Console.ResetColor();
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n无效的选项");
        Console.ResetColor();
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\n按任意键继续...");
    Console.ResetColor();
    Console.ReadKey();
}

/// <summary>
/// 格式化时间间隔为人类可读的格式
/// </summary>
/// <param name="duration">时间间隔</param>
/// <returns>格式化后的时间字符串</returns>
string FormatDuration(TimeSpan duration)
{
    if (duration.TotalHours >= 1)
    {
        return $"{duration.TotalHours:F1}小时";
    }
    else if (duration.TotalMinutes >= 1)
    {
        return $"{duration.TotalMinutes:F1}分钟";
    }
    else if (duration.TotalSeconds >= 1)
    {
        return $"{duration.TotalSeconds:F1}秒";
    }
    else
    {
        return $"{duration.TotalMilliseconds:F0}毫秒";
    }
}

/// <summary>
/// 显示读秒效果
/// </summary>
/// <param name="cancellationToken">取消标记</param>
async Task ShowTickerAsync(CancellationToken cancellationToken)
{
    var elapsed = 0;
    var spinner = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
    var spinnerIndex = 0;

    try
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var seconds = elapsed / 10;
            var tenths = elapsed % 10;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"\r{spinner[spinnerIndex]} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("正在同步数据... ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{seconds}.{tenths}s");
            Console.ResetColor();

            await Task.Delay(100, cancellationToken);
            elapsed++;
            spinnerIndex = (spinnerIndex + 1) % spinner.Length;
        }
    }
    catch (OperationCanceledException)
    {
        // 正常取消，不需要处理
    }
    finally
    {
        Console.ResetColor();
    }
}
