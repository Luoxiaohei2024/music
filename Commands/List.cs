
using CommandSystem;
using NorthwoodLib.Pools;
using NWAPIPermissionSystem;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Events;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security;
using System.Text;

namespace music.Commands
{
    internal class List : ICommand
    {
        public string Command => nameof(List);
        public string Description => "Music list";
        public string[] Aliases => new string[] { };
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                Player player = Player.Get((PlayerCommandSender)sender);
                if (!player.CheckPermission("music.list"))
                {
                    response = "您无权访问此命令";
                    return false;
                }
                if (Plugin.Instance.loadedClipNames.Count == 0)
                {
                    response = "当前没有加载任何音频文件。";
                    return false;
                }

                // 拼接音频文件名
                StringBuilder audioNamesBuilder = new StringBuilder();
                for (int i = 0; i < Plugin.Instance.loadedClipNames.Count; i++)
                {
                    if (i > 0)
                    {
                        audioNamesBuilder.AppendLine(); // 换行分隔每个文件名
                    }
                    audioNamesBuilder.Append(Plugin.Instance.loadedClipNames[i]);
                }

                response = $"当前加载的音频文件列表:\n{audioNamesBuilder.ToString()}";

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                response = $"发生错误: {ex.Message}";
                return false;
            }
        }
    }
}