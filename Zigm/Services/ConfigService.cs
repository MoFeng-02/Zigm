using System.Text.Json;
using Zigm.Languages;
using Zigm.Models;
using Zigm.Services.Interfaces;

namespace Zigm.Services;

/// <summary>
/// 配置服务类，负责处理配置的读取、写入和管理
/// </summary>
public class ConfigService : IConfigService
{
    private const string ConfigFileName = "config.json";
    private readonly string _configFilePath;
    private readonly Lazy<Config> _lazyConfig;
    private Config? _config;

    /// <summary>
    /// 构造函数 - 支持 DI 注入配置路径
    /// </summary>
    /// <param name="configPath">配置文件路径（可选，默认使用应用数据目录）</param>
    public ConfigService(string? configPath = null)
    {
        _configFilePath = ResolveConfigFilePath(configPath);
        _lazyConfig = new Lazy<Config>(LoadConfigCore, LazyThreadSafetyMode.PublicationOnly);
    }

    /// <summary>
    /// 解析配置文件路径
    /// </summary>
    /// <param name="configPath">可选的显式配置路径</param>
    /// <returns>完整的配置文件路径</returns>
    private static string ResolveConfigFilePath(string? configPath)
    {
        if (!string.IsNullOrEmpty(configPath))
        {
            return Path.IsPathRooted(configPath) 
                ? configPath 
                : Path.Combine(AppContext.BaseDirectory, configPath);
        }

        var basePath = Path.Combine(AppContext.BaseDirectory, "ZigmConfig");
        return Path.Combine(basePath, ConfigFileName);
    }

    /// <summary>
    /// 获取配置文件所在目录
    /// </summary>
    public string ConfigDirectory => Path.GetDirectoryName(_configFilePath) ?? string.Empty;

    /// <summary>
    /// 获取配置文件完整路径
    /// </summary>
    public string ConfigFilePath => _configFilePath;

    /// <summary>
    /// 核心配置加载逻辑
    /// </summary>
    /// <returns>加载的配置对象</returns>
    private Config LoadConfigCore()
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
        
        SetDefaultValues(config);
        return config;
    }
    
    /// <summary>
    /// 设置配置的默认值
    /// </summary>
    /// <param name="config">配置对象</param>
    private void SetDefaultValues(Config config)
    {
        if (string.IsNullOrEmpty(config.StoragePath))
        {
            var basePath = Path.GetDirectoryName(_configFilePath) ?? AppContext.BaseDirectory;
            config.StoragePath = Path.Combine(basePath, "zig-versions");
        }
        
        Directory.CreateDirectory(config.StoragePath);
        
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
    /// 初始化配置服务
    /// 如果配置不存在，会创建默认配置
    /// </summary>
    public void Initialize()
    {
        var config = GetConfig();
        if (_lazyConfig.IsValueCreated)
        {
            SaveConfig();
        }
    }

    /// <summary>
    /// 异步初始化配置服务
    /// 如果配置不存在，会创建默认配置
    /// </summary>
    public async Task InitializeAsync()
    {
        var config = GetConfig();
        if (_lazyConfig.IsValueCreated)
        {
            await SaveConfigAsync();
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public void SaveConfig()
    {
        try
        {
            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(GetConfig(), ConfigJsonContext.Default.Config);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            var saveError = AppLang.保存配置文件失败;
            Console.WriteLine(string.Format(saveError, ex.Message));
        }
    }

    /// <summary>
    /// 异步保存配置到文件
    /// </summary>
    public async Task SaveConfigAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(GetConfig(), ConfigJsonContext.Default.Config);
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        catch (Exception ex)
        {
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
        if (_config != null)
        {
            return _config;
        }
        return _lazyConfig.Value;
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
    /// 异步更新配置
    /// </summary>
    /// <param name="config">新的配置对象</param>
    public async Task UpdateConfigAsync(Config config)
    {
        _config = config;
        await SaveConfigAsync();
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
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(GetConfig(), convertedValue);
                SaveConfig();

                var updated = AppLang.配置项已更新;
                Console.WriteLine(string.Format(updated, key, value));
            }
            catch (Exception ex)
            {
                var failed = AppLang.更新配置项失败;
                Console.WriteLine(string.Format(failed, key, ex.Message));
            }
        }
        else
        {
            var unknown = AppLang.未知的配置项;
            Console.WriteLine(string.Format(unknown, key));
        }
    }

    /// <summary>
    /// 异步设置特定配置项
    /// </summary>
    /// <param name="key">配置项键名</param>
    /// <param name="value">配置项值</param>
    public async Task SetConfigValueAsync(string key, string value)
    {
        var property = typeof(Config).GetProperty(key);
        if (property != null)
        {
            try
            {
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(GetConfig(), convertedValue);
                await SaveConfigAsync();

                var updated = AppLang.配置项已更新;
                Console.WriteLine(string.Format(updated, key, value));
            }
            catch (Exception ex)
            {
                var failed = AppLang.更新配置项失败;
                Console.WriteLine(string.Format(failed, key, ex.Message));
            }
        }
        else
        {
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
        var configPathLabel = AppLang.配置文件路径;

        var config = GetConfig();
        Console.WriteLine(string.Format(configPathLabel, _configFilePath));
        Console.WriteLine(string.Format(storagePath, config.StoragePath ?? defaultValue));
        Console.WriteLine(string.Format(autoCheck, config.AutoCheckUpdates));
        Console.WriteLine(string.Format(currentVersion, config.CurrentVersion ?? notSet));
        Console.WriteLine(string.Format(downloadTimeout, config.DownloadTimeout, seconds));
        Console.WriteLine(string.Format(defaultSource, config.DefaultSource));
        Console.WriteLine(string.Format(language, config.Language ?? systemDefault));
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

    /// <summary>
    /// 异步重置配置为默认值
    /// </summary>
    public async Task ResetConfigAsync()
    {
        _config = new Config();
        await SaveConfigAsync();
        Console.WriteLine(AppLang.重置配置为默认值);
    }

    /// <summary>
    /// 检查配置是否已加载
    /// </summary>
    public bool IsConfigLoaded => _lazyConfig.IsValueCreated || _config != null;
}