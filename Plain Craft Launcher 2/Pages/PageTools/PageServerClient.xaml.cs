using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using PCL.Core.Minecraft;
using PCL.Core.UI.Hints;

namespace PCL;

public partial class PageServerClient : IPageView
{
    // ★ 按需改：实例文件夹名 和 jar 文件名
    private const string InstanceName = "GYF";
    private const string JarName = "Updater.jar";

    public PageServerClient()
    {
        InitializeComponent();
        Loaded += (_, _) => Reload();
    }

    public void OnNavigatedTo(object? parameter) => Reload();

    public void OnNavigatedFrom()
    {
    }

    private static string? JarPath =>
        string.IsNullOrEmpty(GameFolders.mcFolderSelected)
            ? null
            : Path.Combine(GameFolders.mcFolderSelected, "versions", InstanceName, JarName);

    public void Reload()
    {
        var jar = JarPath;
        var exists = jar is not null && File.Exists(jar);
        LabStatus.Text = exists ? $"已就绪：{jar}" : "未找到程序文件，请先安装";
        BtnLaunch.IsEnabled = exists;
    }

    private void BtnLaunch_Click(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var jar = JarPath;
            if (jar is null || !File.Exists(jar))
            {
                HintService.Show(Hints.Error("没有找到程序文件，请先安装"));
                Reload();
                return;
            }

            var java = JavaService.JavaManager.GetSortedJavaList()
                .FirstOrDefault(x => x.IsEnabled && x.Installation.IsStillAvailable);
            if (java is null)
            {
                HintService.Show(Hints.Error("没有找到可用的 Java"));
                return;
            }

            var javaExe = java.Installation.JavawExePath ?? java.Installation.JavaExePath;
            Process.Start(new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = $"-jar \"{jar}\"",
                WorkingDirectory = Path.GetDirectoryName(jar)!,
                UseShellExecute = false
            });
            HintService.Show(Hints.Success("已启动"));
        }
        catch (Exception ex)
        {
            Common.Log(ex, "启动程序失败", Common.LogLevel.Feedback);
        }
    }
}
