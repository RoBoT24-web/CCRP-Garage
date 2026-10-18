using System;
using System.Linq;
using System.Threading.Tasks;
using CCRPGarage.DatabaseManagers;
using CCRPGarage.Enums;
using CCRPGarage.Models;
using CCRPGarage.Utils;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities;
using SDG.Unturned;
using RaycastInfo = RFRocketLibrary.Models.RaycastInfo;
using VehicleUtil = CCRPGarage.Utils.VehicleUtil;

namespace CCRPGarage.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("garageadd")]
#if RELEASEPUNCH
    [CommandAliases("vc")]
#else
    [CommandAliases("gadd", "ga")]
#endif
    [CommandInfo("Store vehicle to garage.", "/garageadd [vehicleName]", AllowSimultaneousCalls = false)]
    public class GarageAddCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            var player = (UnturnedPlayer) context.Player;
            var vehicle = player.CurrentVehicle;

            if (CCRPGarage.Plugin.Inst.IsProcessingGarage.TryGetValue(player.CSteamID.m_SteamID,
                    out var lastProcessing) && lastProcessing.HasValue &&
                (DateTime.Now - lastProcessing.Value).TotalSeconds <= 1)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.PROCESSING_GARAGE.ToString()),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            CCRPGarage.Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = null;
            if (vehicle == null)
            {
                var raycastInfo = await ThreadTool.RunOnGameThreadAsync(() =>
                    RaycastInfo.FromPlayerLook(player.Player, RayMasks.VEHICLE));
                if (raycastInfo?.Vehicle == null)
                {
                    await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.NO_VEHICLE_INPUT.ToString()),
                        CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                    return;
                }

                vehicle = raycastInfo.Vehicle;
            }

            if (vehicle.isDead || vehicle.isExploded)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.NO_VEHICLE_INPUT.ToString()),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (vehicle.lockedOwner.m_SteamID != player.CSteamID.m_SteamID)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.VEHICLE_NOT_OWNED.ToString()),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            var slot = player.GetGarageSlot();
            if (GarageManager.Count(player.CSteamID.m_SteamID) >= slot)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.GARAGE_FULL.ToString(), slot),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!CCRPGarage.Plugin.Conf.AllowTrain && vehicle.asset.engine == EEngine.TRAIN)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.TRAIN_NOT_ALLOWED.ToString()),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (BlacklistUtil.IsVehicleBlacklisted(vehicle, player.HasPermission))
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName, vehicle.asset.id), CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            var blacklisted = BlacklistUtil.FindViolation(vehicle, player.HasPermission);
            if (blacklisted != null)
            {
                var hit = blacklisted.Value;
                await context.ReplyAsync(VehicleUtil.TranslateRich((hit.IsBarricade ? EResponse.BLACKLIST_BARRICADE : EResponse.BLACKLIST_ITEM).ToString(), hit.Asset.itemName, hit.Asset.id), CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            await ThreadTool.RunOnGameThreadAsync(() => { vehicle.forceRemoveAllPlayers(); });
            CCRPGarage.Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = DateTime.Now;
            CCRPGarage.Plugin.Inst.BusyVehicle.Add(vehicle.instanceID);
            var vehicleName = context.CommandRawArguments.Length == 0 ? vehicle.asset.vehicleName : string.Join(" ", context.CommandRawArguments);
            var garageContent = VehicleWrapper.Create(vehicle);
            await ThreadTool.RunOnGameThreadAsync(() =>
            {
                VehicleUtil.ClearTrunkAndBarricades(vehicle);
                VehicleManager.askVehicleDestroy(vehicle);
            });
            await DatabaseManager.Queue.Enqueue(async () =>
                await GarageManager.AddAsync(new PlayerGarage
                {
                    SteamId = player.CSteamID.m_SteamID,
                    VehicleName = vehicleName,
                    GarageContent = garageContent,
                    LastUpdated = DateTime.Now,
                })
            )!;
            CCRPGarage.Plugin.Inst.BusyVehicle.Remove(vehicle.instanceID);
            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.GARAGE_ADDED.ToString(), vehicleName),
                CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
            return;
        }
    }
}
