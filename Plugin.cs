using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace music
{
    public class Plugin
    {
        private string folderPath => Path.Combine(FileManager.GetAppFolder(false, false, ""), $"PluginAPI/plugins/{Server.Port}/Music/");

        // 用于存储已加载的音频剪辑名称
        public List<string> loadedClipNames = new List<string>();
        public static Plugin Instance { get; set; }
        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("Music", "1.0", "ANBK", "罗小黑")]
        public void OnEnabled()
        {
            Instance = this;
            EventManager.RegisterEvents(this);
            Log.Info("正常启动");
            try
            {
                // 检查文件夹是否存在
                if (!Directory.Exists(folderPath))
                {
                    Log.Error($"The specified folder {folderPath} does not exist.");
                    return;
                }

                // 获取文件夹内所有 .ogg 文件
                string[] oggFiles = Directory.GetFiles(folderPath, "*.ogg");

                foreach (string filePath in oggFiles)
                {
                    // 获取文件名（不包含扩展名）
                    string clipName = Path.GetFileNameWithoutExtension(filePath);

                    // 调用 AudioClipStorage 加载音频
                    AudioClipStorage.LoadClip(filePath, clipName);
                    AudioClipStorage.DestroyClip(clipName);
                    // 记录已加载的音频剪辑名称
                    loadedClipNames.Add(clipName);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred while loading audio files: {ex.Message}");
            }
        }

        public void Reload()
        {
            try
            {
                // 销毁之前加载的所有音频剪辑
                foreach (string clipName in loadedClipNames)
                {
                    AudioClipStorage.DestroyClip(clipName);
                }
                // 清空已加载音频剪辑名称列表
                loadedClipNames.Clear();

                // 检查文件夹是否存在
                if (!Directory.Exists(folderPath))
                {
                    Log.Error($"The specified folder {folderPath} does not exist.");
                    return;
                }

                // 获取文件夹内所有 .ogg 文件
                string[] oggFiles = Directory.GetFiles(folderPath, "*.ogg");

                foreach (string filePath in oggFiles)
                {
                    try
                    {
                        // 获取文件名（不包含扩展名）
                        string clipName = Path.GetFileNameWithoutExtension(filePath);

                        // 检查是否已经加载过该音频剪辑
                        if (loadedClipNames.Contains(clipName))
                        {
                            Log.Warning($"The audio clip {clipName} has already been loaded, skipping.");
                            continue;
                        }

                        // 调用 AudioClipStorage 加载音频
                        AudioClipStorage.LoadClip(filePath, clipName);

                        // 记录已加载的音频剪辑名称
                        loadedClipNames.Add(clipName);
                    }
                    catch (Exception innerEx)
                    {
                        Log.Error($"An error occurred while loading audio file {filePath}: {innerEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred while reloading audio files: {ex.Message}");
            }
        }
    }
}
