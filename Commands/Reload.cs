using CommandSystem;
using NWAPIPermissionSystem;
using PluginAPI.Core;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace music.Commands
{
    internal class Reload : ICommand
    {
        public string Command => nameof(Reload);
        public string Description => "Reload Music";
        public string[] Aliases => new string[] { };
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {


                try
                {
                    // 检查 sender 是否可以转换为 PlayerCommandSender 类型
                    if (sender is not PlayerCommandSender playerCommandSender)
                    {
                        response = "无法获取玩家命令发送者信息。";
                        return false;
                    }

                    Player player = Player.Get(playerCommandSender);
                    if (player == null)
                    {
                        response = "无法获取对应的玩家信息。";
                        return false;
                    }

                    if (!player.CheckPermission("music.play"))
                    {
                        response = "You do not have permission to access this command.\r\n";
                        return false;
                    }

                new Task(() =>{Plugin.Instance.Reload();}).Start();

                    response = $"Reload";
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
