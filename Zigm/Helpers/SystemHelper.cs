namespace Zigm.Helpers;

public class SystemHelper
{

    /// <summary>
    /// 获取当前系统的架构标识
    /// </summary>
    /// <returns>系统架构标识，如 x86_64-windows</returns>
    public static string GetSystemArchitecture()
    {
        string os;
        if (OperatingSystem.IsWindows())
        {
            os = "windows";
        }
        else if (OperatingSystem.IsLinux())
        {
            os = "linux";
        }
        else if (OperatingSystem.IsMacOS())
        {
            os = "macos";
        }
        else
        {
            os = "unknown";
        }

        string arch;
        if (Environment.Is64BitOperatingSystem)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
            {
                arch = "aarch64";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64)
            {
                arch = "x86_64";
            }
            else
            {
                arch = "unknown";
            }
        }
        else
        {
            arch = "x86";
        }

        // 根据JSON结构，系统架构的格式是 "arch-os"，如 "x86_64-windows"
        return $"{arch}-{os}";
    }

}
