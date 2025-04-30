using Npgsql;
using SyncDataFromProductionEnv.Models;

namespace SyncDataFromProductionEnv.Services;

/// <summary>
/// 数据库服务类，负责与PostgreSQL数据库的交互
/// </summary>
public class DatabaseService
{
    private readonly string _connectionString;
    private const int MaxRetryCount = 3;
    private const int RetryDelayMs = 1000;

    /// <summary>
    /// 构造函数，初始化数据库连接字符串
    /// </summary>
    public DatabaseService()
    {
        _connectionString = "";
    }

    /// <summary>
    /// 获取所有未删除的同步配置
    /// </summary>
    /// <returns>同步配置列表</returns>
    public async Task<List<SyncDefinition>> GetAllSyncDefinitionsAsync()
    {
        var definitions = new List<SyncDefinition>();
        var retryCount = 0;

        while (retryCount < MaxRetryCount)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(
                    "SELECT id, key, tables, created_time, updated_time, is_delete FROM mask.sync_definition WHERE is_delete = false",
                    connection);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    definitions.Add(new SyncDefinition
                    {
                        Id = reader.GetInt32(0),
                        Key = reader.GetString(1),
                        Tables = reader.GetFieldValue<string[]>(2),
                        CreatedTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        UpdatedTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        IsDelete = reader.GetBoolean(5)
                    });
                }

                return definitions;
            }
            catch (Exception ex) when (retryCount < MaxRetryCount - 1)
            {
                retryCount++;
                await Task.Delay(RetryDelayMs * retryCount);
                Console.WriteLine($"获取同步配置失败，正在重试 ({retryCount}/{MaxRetryCount}): {ex.Message}");
            }
        }

        throw new Exception($"获取同步配置失败，已重试{MaxRetryCount}次");
    }

    /// <summary>
    /// 获取指定日期的所有key
    /// </summary>
    /// <param name="date">日期，格式：yyyyMMdd</param>
    /// <returns>key列表</returns>
    private async Task<List<string>> GetKeysByDateAsync(string date)
    {
        var keys = new List<string>();
        var retryCount = 0;

        while (retryCount < MaxRetryCount)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(
                    "SELECT key FROM mask.sync_definition WHERE key LIKE @prefix AND is_delete = false",
                    connection);

                command.Parameters.AddWithValue("@prefix", $"Newcats{date}%");

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    keys.Add(reader.GetString(0));
                }

                return keys;
            }
            catch (Exception ex) when (retryCount < MaxRetryCount - 1)
            {
                retryCount++;
                await Task.Delay(RetryDelayMs * retryCount);
                Console.WriteLine($"获取key列表失败，正在重试 ({retryCount}/{MaxRetryCount}): {ex.Message}");
            }
        }

        throw new Exception($"获取key列表失败，已重试{MaxRetryCount}次");
    }

    /// <summary>
    /// 添加新的同步配置
    /// </summary>
    /// <param name="tables">要同步的表名列表，多个表名用逗号分隔</param>
    /// <param name="key">可选的Key，如果不指定则自动生成</param>
    /// <returns>新创建的配置的Key</returns>
    public async Task<string> AddSyncDefinitionAsync(string tables, string? key = null)
    {
        var retryCount = 0;
        while (retryCount < MaxRetryCount)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // 如果没有指定key，则自动生成
                    var finalKey = key;
                    if (string.IsNullOrEmpty(finalKey))
                    {
                        finalKey = await GenerateKeyAsync();
                    }
                    else
                    {
                        // 检查指定的key是否已存在
                        await using var checkCommand = new NpgsqlCommand(
                            "SELECT COUNT(1) FROM mask.sync_definition WHERE key = @key AND is_delete = false",
                            connection,
                            transaction);

                        checkCommand.Parameters.AddWithValue("@key", finalKey);
                        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            throw new Exception($"Key '{finalKey}' 已存在");
                        }
                    }

                    await using var command = new NpgsqlCommand(
                        "INSERT INTO mask.sync_definition (key, tables, created_time, updated_time, is_delete) VALUES (@key, @tables, @createdTime, @updatedTime, false) RETURNING key",
                        connection,
                        transaction);

                    var now = DateTime.Now;
                    command.Parameters.AddWithValue("@key", finalKey);
                    command.Parameters.AddWithValue("@tables", tables.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    command.Parameters.AddWithValue("@createdTime", now);
                    command.Parameters.AddWithValue("@updatedTime", now);

                    var result = (string)await command.ExecuteScalarAsync()!;
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex) when (retryCount < MaxRetryCount - 1)
            {
                retryCount++;
                await Task.Delay(RetryDelayMs * retryCount);
                Console.WriteLine($"添加同步配置失败，正在重试 ({retryCount}/{MaxRetryCount}): {ex.Message}");
            }
        }

        throw new Exception($"添加同步配置失败，已重试{MaxRetryCount}次");
    }

    /// <summary>
    /// 生成新的同步配置Key
    /// 规则：Newcats + 年月日 + 字母后缀（A-Z）
    /// </summary>
    /// <returns>生成的Key</returns>
    private async Task<string> GenerateKeyAsync()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var existingKeys = await GetKeysByDateAsync(date);

        var suffix = 'A';
        while (existingKeys.Contains($"Newcats{date}{suffix}"))
        {
            suffix++;
            if (suffix > 'Z')
            {
                throw new Exception("当天生成的Key数量已超过限制");
            }
        }

        return $"Newcats{date}{suffix}";
    }

    /// <summary>
    /// 更新同步配置的Key
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <param name="newKey">新的Key</param>
    /// <returns>更新任务</returns>
    public async Task UpdateSyncDefinitionKeyAsync(int id, string newKey)
    {
        var retryCount = 0;
        while (retryCount < MaxRetryCount)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // 检查新Key是否已存在
                    await using var checkCommand = new NpgsqlCommand(
                        "SELECT COUNT(1) FROM mask.sync_definition WHERE key = @key AND id != @id AND is_delete = false",
                        connection,
                        transaction);

                    checkCommand.Parameters.AddWithValue("@key", newKey);
                    checkCommand.Parameters.AddWithValue("@id", id);

                    var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                    if (count > 0)
                    {
                        throw new Exception($"Key '{newKey}' 已存在");
                    }

                    // 更新Key
                    await using var updateCommand = new NpgsqlCommand(
                        "UPDATE mask.sync_definition SET key = @key, updated_time = @updatedTime WHERE id = @id AND is_delete = false",
                        connection,
                        transaction);

                    updateCommand.Parameters.AddWithValue("@key", newKey);
                    updateCommand.Parameters.AddWithValue("@id", id);
                    updateCommand.Parameters.AddWithValue("@updatedTime", DateTime.Now);

                    var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        throw new Exception("未找到要更新的配置或配置已被删除");
                    }

                    await transaction.CommitAsync();
                    return;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex) when (retryCount < MaxRetryCount - 1)
            {
                retryCount++;
                await Task.Delay(RetryDelayMs * retryCount);
                Console.WriteLine($"更新配置Key失败，正在重试 ({retryCount}/{MaxRetryCount}): {ex.Message}");
            }
        }

        throw new Exception($"更新配置Key失败，已重试{MaxRetryCount}次");
    }

    /// <summary>
    /// 删除同步配置（软删除）
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>删除任务</returns>
    public async Task DeleteSyncDefinitionAsync(int id)
    {
        var retryCount = 0;
        while (retryCount < MaxRetryCount)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // 检查配置是否存在且未删除
                    await using var checkCommand = new NpgsqlCommand(
                        "SELECT COUNT(1) FROM mask.sync_definition WHERE id = @id AND is_delete = false",
                        connection,
                        transaction);

                    checkCommand.Parameters.AddWithValue("@id", id);
                    var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                    if (count == 0)
                    {
                        throw new Exception("未找到要删除的配置或配置已被删除");
                    }

                    // 软删除配置
                    await using var deleteCommand = new NpgsqlCommand(
                        "UPDATE mask.sync_definition SET is_delete = true, updated_time = @updatedTime WHERE id = @id",
                        connection,
                        transaction);

                    deleteCommand.Parameters.AddWithValue("@id", id);
                    deleteCommand.Parameters.AddWithValue("@updatedTime", DateTime.Now);

                    await deleteCommand.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                    return;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex) when (retryCount < MaxRetryCount - 1)
            {
                retryCount++;
                await Task.Delay(RetryDelayMs * retryCount);
                Console.WriteLine($"删除配置失败，正在重试 ({retryCount}/{MaxRetryCount}): {ex.Message}");
            }
        }

        throw new Exception($"删除配置失败，已重试{MaxRetryCount}次");
    }
}