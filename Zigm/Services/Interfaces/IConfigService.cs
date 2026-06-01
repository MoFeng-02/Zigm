using Zigm.Models;

namespace Zigm.Services.Interfaces;

/// <summary>
/// 配置服务接口，负责处理应用程序配置的读取、写入和管理
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// 获取配置文件的完整路径
    /// </summary>
    string ConfigFilePath { get; }

    /// <summary>
    /// 获取配置文件所在目录的路径
    /// </summary>
    string ConfigDirectory { get; }

    /// <summary>
    /// 获取配置是否已加载的状态
    /// </summary>
    bool IsConfigLoaded { get; }

    /// <summary>
    /// 初始化配置服务，确保配置文件存在并加载配置
    /// </summary>
    void Initialize();

    /// <summary>
    /// 异步初始化配置服务，确保配置文件存在并加载配置
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 保存当前配置到配置文件
    /// </summary>
    void SaveConfig();

    /// <summary>
    /// 异步保存当前配置到配置文件
    /// </summary>
    Task SaveConfigAsync();

    /// <summary>
    /// 获取当前配置对象
    /// </summary>
    /// <returns>配置对象</returns>
    Config GetConfig();

    /// <summary>
    /// 更新配置对象
    /// </summary>
    /// <param name="config">新的配置对象</param>
    void UpdateConfig(Config config);

    /// <summary>
    /// 异步更新配置对象
    /// </summary>
    /// <param name="config">新的配置对象</param>
    Task UpdateConfigAsync(Config config);

    /// <summary>
    /// 设置指定配置项的值
    /// </summary>
    /// <param name="key">配置项键名</param>
    /// <param name="value">配置项值</param>
    void SetConfigValue(string key, string value);

    /// <summary>
    /// 异步设置指定配置项的值
    /// </summary>
    /// <param name="key">配置项键名</param>
    /// <param name="value">配置项值</param>
    Task SetConfigValueAsync(string key, string value);

    /// <summary>
    /// 在控制台显示当前配置信息
    /// </summary>
    void ShowConfig();

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    void ResetConfig();

    /// <summary>
    /// 异步重置配置为默认值
    /// </summary>
    Task ResetConfigAsync();
}
