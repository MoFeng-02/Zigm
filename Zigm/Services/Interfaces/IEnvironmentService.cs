namespace Zigm.Services.Interfaces;

/// <summary>
/// 环境服务接口，负责处理系统环境变量的管理和修改
/// </summary>
public interface IEnvironmentService
{
    /// <summary>
    /// 获取系统的 PATH 环境变量
    /// </summary>
    /// <param name="target">环境变量目标（用户级或系统级）</param>
    /// <returns>PATH 环境变量的值</returns>
    string GetSystemPath(EnvironmentVariableTarget target = EnvironmentVariableTarget.User);

    /// <summary>
    /// 设置系统的 PATH 环境变量
    /// </summary>
    /// <param name="newPath">新的 PATH 环境变量值</param>
    /// <param name="target">环境变量目标（用户级或系统级）</param>
    void SetSystemPath(string newPath, EnvironmentVariableTarget target = EnvironmentVariableTarget.User);

    /// <summary>
    /// 将指定版本的 Zig 添加到系统 PATH 环境变量中
    /// </summary>
    /// <param name="version">Zig 版本号</param>
    /// <param name="target">环境变量目标（用户级或系统级）</param>
    /// <returns>是否成功添加</returns>
    bool AddZigToPath(string version, EnvironmentVariableTarget target = EnvironmentVariableTarget.User);

    /// <summary>
    /// 刷新当前进程的环境变量
    /// </summary>
    /// <param name="target">环境变量目标（用户级或系统级）</param>
    void RefreshEnvironment(EnvironmentVariableTarget target = EnvironmentVariableTarget.User);

    /// <summary>
    /// 显示设置 PATH 环境变量的官方建议
    /// </summary>
    void ShowOfficialPathSetupInfo();

    /// <summary>
    /// 检查当前进程是否具有 Windows 管理员权限
    /// </summary>
    /// <returns>如果具有管理员权限返回 true，否则返回 false</returns>
    bool CheckAdminPrivileges();

    /// <summary>
    /// 检查是否可以修改指定目标的环境变量
    /// </summary>
    /// <param name="target">环境变量目标（用户级或系统级）</param>
    /// <returns>如果可以修改返回 true，否则返回 false</returns>
    bool CanModifyEnvironmentVariable(EnvironmentVariableTarget target);
}
