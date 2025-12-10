using System.Text.Json;
using Zigm.Languages;
using Zigm.Models;

namespace Zigm.Services;

/// <summary>
/// 配置服务类，负责处理配置的读取、写入和管理
/// </summary>
public class ConfigService
{
    private const string ConfigFileName = "config.json";
    private readonly string _configFilePath;
    private Config _config;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ConfigService()
    {
        
        // 获取配置文件路径
        var basePath = GetBasePath();
        _configFilePath = Path.Combine(basePath, ConfigFileName);
        
        // 加载配置
        _config = LoadConfig();
        
        // 确保配置文件存在
        SaveConfig();
    }

    /// <summary>
    /// 获取配置文件的基础路径
    /// </summary>
    /// <returns>配置文件的基础路径</returns>
    private string GetBasePath()
    {
        // 使用程序自身所在的目录
        var appDirectory = AppContext.BaseDirectory;
        return Path.Combine(appDirectory, "ZigmConfig");
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>配置对象</returns>
    private Config LoadConfig()
    {
        Config config;
        
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                config = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.Config) ?? new Config();
            }
            else
            {
                config = new Config();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format(AppLang.加载配置文件失败, ex.Message));
            Console.WriteLine(AppLang.将使用默认配置);
            config = new Config();
        }
        
        // 设置默认值（如果未配置）
        SetDefaultValues(config);
        
        return config;
    }
    
    /// <summary>
    /// 设置配置的默认值
    /// </summary>
    /// <param name="config">配置对象</param>
    private void SetDefaultValues(Config config)
    {
        // 设置默认存储路径
        if (string.IsNullOrEmpty(config.StoragePath))
        {
            var basePath = GetBasePath();
            config.StoragePath = Path.Combine(basePath, "zig-versions");
        }
        
        // 确保存储目录存在
        Directory.CreateDirectory(config.StoragePath);
        
        // 设置其他默认值（如果未配置）
        if (config.DownloadTimeout <= 0)
        {
            config.DownloadTimeout = 300;
        }
        
        if (string.IsNullOrEmpty(config.DefaultSource))
        {
            config.DefaultSource = "official";
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public void SaveConfig()
    {
        try
        {
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath) ?? string.Empty);
            
            var json = JsonSerializer.Serialize(_config, ConfigJsonContext.Default.Config);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            // 获取资源字符串
            var saveError = AppLang.保存配置文件失败;
            Console.WriteLine(string.Format(saveError, ex.Message));
        }
    }

    /// <summary>
    /// 获取当前配置
    /// </summary>
    /// <returns>当前配置对象</returns>
    public Config GetConfig()
    {
        return _config;
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="config">新的配置对象</param>
    public void UpdateConfig(Config config)
    {
        _config = config;
        SaveConfig();
    }

    /// <summary>
    /// 设置特定配置项
    /// </summary>
    /// <param name="key">配置项键名</param>
    /// <param name="value">配置项值</param>
    public void SetConfigValue(string key, string value)
    {
        var property = typeof(Config).GetProperty(key);
        if (property != null)
        {
            try
            {
                // 转换值类型
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(_config, convertedValue);
                SaveConfig();

                // 获取资源字符串
                var updated = AppLang.配置项已更新;
                Console.WriteLine(string.Format(updated, key, value));
            }
            catch (Exception ex)
            {
                // 获取资源字符串
                var failed = AppLang.更新配置项失败;
                Console.WriteLine(string.Format(failed, key, ex.Message));
            }
        }
        else
        {
            // 获取资源字符串
            var unknown = AppLang.未知的配置项;
            Console.WriteLine(string.Format(unknown, key));
        }
    }

    /// <summary>
    /// 显示当前配置
    /// </summary>
    public void ShowConfig()
    {
        Console.WriteLine(AppLang.显示当前配置);
        Console.WriteLine("---------------------------------");
        
        // 获取资源字符串
        var storagePath = AppLang.存储路径;
        var autoCheck = AppLang.自动检查更新;
        var currentVersion = AppLang.当前版本配置;
        var downloadTimeout = AppLang.下载超时;
        var seconds = AppLang.秒;
        var defaultSource = AppLang.默认源;
        var language = AppLang.语言设置;
        var defaultValue = AppLang.默认;
        var notSet = AppLang.未设置;
        var systemDefault = AppLang.系统默认;

        // 输出配置信息
        Console.WriteLine(string.Format(storagePath, _config.StoragePath ?? defaultValue));
        Console.WriteLine(string.Format(autoCheck, _config.AutoCheckUpdates));
        Console.WriteLine(string.Format(currentVersion, _config.CurrentVersion ?? notSet));
        Console.WriteLine(string.Format(downloadTimeout, _config.DownloadTimeout, seconds));
        Console.WriteLine(string.Format(defaultSource, _config.DefaultSource));
        Console.WriteLine(string.Format(language, _config.Language ?? systemDefault));
        Console.WriteLine("---------------------------------");
    }

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    public void ResetConfig()
    {
        _config = new Config();
        SaveConfig();
        Console.WriteLine(AppLang.重置配置为默认值);
    }
}