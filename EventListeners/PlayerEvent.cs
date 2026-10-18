#if RELEASEPUNCH

using System;
using System.Linq;
using CCRPGarage.DatabaseManagers;
using CCRPGarage.Enums;
using CCRPGarage.Models;
using CCRPGarage.Utils;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Utilities;
using SDG.Unturned;
using RaycastInfo = RFRocketLibrary.Models.RaycastInfo;
using VehicleUtil = CCRPGarage.Utils.VehicleUtil;

namespace CCRPGarage.EventListeners
{
    internal static class PlayerEvent
    {
        internal static void OnPunched(Player player, EPlayerPunch punch)
        {
            var uPlayer = UnturnedPlayer.FromPlayer(player);
            var raycastInfo = RaycastInfo.FromPlayerLook(player, RayMasks.VEHICLE, 1.75f);
            if (raycastInfo.Vehicle == null)
                return;

            var cPlayer = player.GetComponent<PlayerComponent>();
            if (cPlayer.PunchCount == 0)
            {
                cPlayer.PunchCount++;
                cPlayer.ResetPunchCount();
                return;
            }

            cPlayer.PunchCount++;
            if (cPlayer.PunchCount == 3)
            {
                if (cPlayer.ResetPunchCor != null)
                {
                    Plugin.Inst.StopCoroutine(cPlayer.ResetPunchCor);
                    cPlayer.ResetPunchCor = null;
                }

                cPlayer.PunchCount = 0;

                if (Plugin.Inst.IsProcessingGarage.TryGetValue(player.channel.owner.playerID.steamID.m_SteamID, out var lastProcessing) && lastProcessing.HasValue && (DateTime.Now - lastProcessing.Value).TotalSeconds <= 1)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.PROCESSING_GARAGE.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }
            
                Plugin.Inst.IsProcessingGarage[player.channel.owner.playerID.steamID.m_SteamID] = null;
                
                var vehicle = raycastInfo.Vehicle;
                if (vehicle.isDead || vehicle.isExploded)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.NO_VEHICLE_INPUT.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }
                
                if (vehicle.lockedOwner.m_SteamID != uPlayer.CSteamID.m_SteamID)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.VEHICLE_NOT_OWNED.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                var slot = uPlayer.GetGarageSlot();
                if (GarageManager.Count(uPlayer.CSteamID.m_SteamID) >= slot)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.GARAGE_FULL.ToString(), slot),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                if (!Plugin.Conf.AllowTrain && vehicle.asset.engine == EEngine.TRAIN)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.TRAIN_NOT_ALLOWED.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                if (BlacklistUtil.IsVehicleBlacklisted(vehicle, uPlayer.HasPermission))
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName, vehicle.asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                var blacklisted = BlacklistUtil.FindViolation(vehicle, uPlayer.HasPermission);
                if (blacklisted != null)
                {
                    var hit = blacklisted.Value;
                    ChatHelper.Say(player, VehicleUtil.TranslateRich((hit.IsBarricade ? EResponse.BLACKLIST_BARRICADE : EResponse.BLACKLIST_ITEM).ToString(), hit.Asset.itemName, hit.Asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                Plugin.Inst.IsProcessingGarage[player.channel.owner.playerID.steamID.m_SteamID] = DateTime.Now;
                vehicle.forceRemoveAllPlayers();
                VehicleUtil.ClearItems(vehicle);
                VehicleManager.askVehicleDestroy(vehicle);
                var garageContent = VehicleWrapper.Create(vehicle);
                DatabaseManager.Queue.Enqueue(async () =>
                {
                    var vehicleName = vehicle.asset.vehicleName;
                    await GarageManager.AddAsync(new PlayerGarage
                    {
                        SteamId = uPlayer.CSteamID.m_SteamID,
                        VehicleName = vehicleName,
                        GarageContent = garageContent,
                        LastUpdated = DateTime.Now,
                    });
                    await ThreadTool.RunOnGameThreadAsync(() =>
                    {
                        ChatHelper.Say(player,
                            VehicleUtil.TranslateRich(EResponse.GARAGE_ADDED.ToString(), vehicleName),
                            Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    });
                });
            }
        }
    }
}
#endif
