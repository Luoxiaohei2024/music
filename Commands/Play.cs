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
using UnityEngine;

namespace music.Commands
{
    internal class Play : ICommand
    {
        public string Command => nameof(Play);
        public string Description => "播放音频";
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

                if (Plugin.Instance.loadedClipNames.Count == 0)
                {
                    response = "There are currently no audio files loaded.";
                    return false;
                }

                if (arguments.Count < 2)
                {
                    response = "The command format is incorrect. The correct format is: Play <Audio File> <Type: Global/Spectator/PlayerAttached>";
                    return false;
                }

                string audioFileName = arguments.At(0);
                string type = arguments.At(1);


                // 检查指定的音频文件是否已加载
                if (!Plugin.Instance.loadedClipNames.Contains(audioFileName))
                {
                    response = $"The specified audio file {audioFileName} is not loaded.";
                    return false;
                }

                if (type.Equals("Global", StringComparison.OrdinalIgnoreCase))
                {
                    AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Global AudioPlayer", onIntialCreation: (p) =>
                    {
                        
                        // This created speaker will be in 2D space ( audio will be always playing directly on you not from specific location ) but make sure that max distance is set to some higher value.
                        Speaker speaker = p.AddSpeaker("Main", isSpatial: false, maxDistance: 5000f);
                    });

                    audioPlayer.AddClip(audioFileName);
                    response = $"The audio file {audioFileName} has been added to the global audio player.";
                    return true;
                }
                else if (type.Equals("Spectator", StringComparison.OrdinalIgnoreCase))
                {
                    AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Spectator AudioPlayer", condition: (hub) =>
                    {
                        // Only players which have spectator role will hear this sound.
                        return hub.roleManager.CurrentRole.RoleTypeId == PlayerRoles.RoleTypeId.Spectator;
                    }, onIntialCreation: (p) =>
                    {
                        // This created speaker will be in 2D space ( audio will be always playing directly on you not from specific location ) but make sure that max distance is set to some higher value.
                        Speaker speaker = p.AddSpeaker("Main", isSpatial: false, maxDistance: 5000f);
                    });

                    audioPlayer.AddClip(audioFileName);

                    response = $"The audio file {audioFileName} has been added to the Spectator audio player.";
                    return true;
                }
                else if (type.Equals("PlayerAttached", StringComparison.OrdinalIgnoreCase))
                {
                    if (arguments.Count < 3)
                    {
                        response = "The command format for PlayerAttached type is incorrect. The correct format is: Play <Audio File> PlayerAttached <playerid>";
                        return false;
                    }
                    string playerId = arguments.At(2);
                    if (playerId.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {

                        foreach (var p in Player.GetPlayers())
                        {
                            CreateForPlayer(p, audioFileName);
                        }
                        response = $"The audio file {audioFileName} has been added to all players' audio players.";
                        return true;
                    }
                    else if (int.TryParse(playerId, out int parsedId))
                    {

                        var targetPlayer = Player.GetPlayers().FirstOrDefault(p => p.PlayerId == parsedId);
                        if (targetPlayer != null)
                        {
                            CreateForPlayer(targetPlayer, audioFileName);
                            response = $"The audio file {audioFileName} has been added to player {targetPlayer.Nickname}'s audio player.";
                            return true;
                        }
                        else
                        {
                            response = $"No player found with ID: {playerId}";
                            return false;
                        }
                    }
                    else
                    {
                        response = $"Invalid player ID format: {playerId}";
                        return false;
                    }
                }
                else
                {
                    response = $"The type {type} is not supported.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                response = $"发生错误: {ex.Message}";
                return false;
            }
        }

        public void CreateForPlayer(Player player, string audioFileName)
        {
            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Player {player.PlayerId}", onIntialCreation: (p) =>
            {
                // Attach created audio player to player.
                p.transform.parent = player.GameObject.transform;

                // This created speaker will be in 3D space.
                Speaker speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 5f, maxDistance: 15f);

                // Attach created speaker to player.
                speaker.transform.parent = player.GameObject.transform;

                // Set local positino to zero to make sure that speaker is in player.
                speaker.transform.localPosition = Vector3.zero;
            });

            // As example we will add clip
            audioPlayer.AddClip(audioFileName);
        }
    }
}