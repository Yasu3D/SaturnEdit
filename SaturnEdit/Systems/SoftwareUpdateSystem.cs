using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Octokit;
using FileMode = System.IO.FileMode;

namespace SaturnEdit.Systems;

public static class SoftwareUpdateSystem
{
    public static void Initialize()
    {
        gitHubClient = new(new ProductHeaderValue("yasu3d-saturn-edit-updater"));
    }

    private static GitHubClient? gitHubClient = null;
    
    private const string RepositoryOwner = "Yasu3D";
    private const string RepositoryName = "SaturnEdit";

    private const string AssetNameWindows = "Windows-x64.zip";
    private const string AssetNameLinux = "Linux-x64.zip";
    
    private static string DownloadPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaturnEdit/update.zip");
    private static string ExtractedDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaturnEdit/extract");

    public static async Task<bool> UpdateAvailable()
    {
        try
        {
            if (!SettingsSystem.EditorSettings.CheckForUpdates)
            {
                return false;
            }
            
            if (gitHubClient == null) return false;

            Release? latestRelease = await gitHubClient.Repository.Release.GetLatest(RepositoryOwner, RepositoryName);
            if (latestRelease == null) return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            string latestVersionString = latestRelease.TagName.Replace("v", "");
            string currentVersionString = versionInfo.FileVersion ?? "";

            if (!Version.TryParse(latestVersionString, out Version? latestVersion)) return false;
            if (!Version.TryParse(currentVersionString, out Version? currentVersion)) return false;

            return latestVersion > currentVersion;
        }
        catch (Exception ex)
        {
            // Don't throw.
            if (ex is not NotFoundException)
            {
                Console.WriteLine(ex);
            }
        }
        
        return false;
    }

    public static async Task<(bool, string)> Update()
    {
        try
        {
            if (gitHubClient == null) return (false, "ModalDialog.Update.Error.GitHubNotFound");

            // Get latest release.
            Release? latestRelease = null;
            try
            {
                latestRelease = await gitHubClient.Repository.Release.GetLatest(RepositoryOwner, RepositoryName);
            }
            catch
            {
                 // Don't throw.
            }
            
            if (latestRelease == null) return (false, "ModalDialog.Update.Error.ReleaseNotFound");

            // Get correct asset based on platform.
            ReleaseAsset? asset = null;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                asset = latestRelease.Assets.FirstOrDefault(x => x.Name == AssetNameWindows);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                asset = latestRelease.Assets.FirstOrDefault(x => x.Name == AssetNameLinux);
            }

            if (asset == null) return (false, "ModalDialog.Update.Error.AssetNotFound");

            // Download asset.
            using (HttpClient httpClient = new())
            {
                await using (Stream stream = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl))
                {
                    await using (FileStream fileStream = new(DownloadPath, FileMode.OpenOrCreate))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
            
            // Unzip.
            ZipFile.ExtractToDirectory(DownloadPath, ExtractedDirectory);
            
            // Get some paths.
            string processPath = Environment.ProcessPath ?? "";
            if (processPath == "")
            {
                File.Delete(DownloadPath);
                Directory.Delete(ExtractedDirectory);
                return (false, "ModalDialog.Update.Error.ProcessNotFound");
            }

            string updaterProcessPath = Path.GetDirectoryName(processPath) ?? "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                updaterProcessPath = Path.Combine(updaterProcessPath, "SaturnEditUpdater.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                updaterProcessPath = Path.Combine(updaterProcessPath, "SaturnEditUpdater");
            }
            
            ProcessStartInfo updaterProcess = new()
            {
                FileName = updaterProcessPath,
                ArgumentList = { processPath, DownloadPath, ExtractedDirectory },
                UseShellExecute = true,
            };

            Process.Start(updaterProcess);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        return (false, "ModalDialog.Update.Error.Unknown");
    }
}