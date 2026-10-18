using System;
using System.Linq;
using System.Threading.Tasks;
using CCRPGarage.DatabaseManagers;
using CCRPGarage.Enums;
using CCRPGarage.Models;
using CCRPGarage.Utils;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Core.Logging;
using SDG.Unturned;
using Steamworks;
using VehicleUtil = CCRPGarage.Utils.VehicleUtil;

namespace CCRPGarage.EventListeners
{
    internal static class VehicleEvent
    {
        internal static void OnExploded(InteractableVehicle vehicle)
        {
            if (!Plugin.Conf.AutoClearDestroyedVehicles)
                return;

            VehicleManager.askVehicleDestroy(vehicle);
        }

        internal static void OnPreVehicleDestroyed(InteractableVehicle vehicle, ref bool shouldallow)
        {
            if (!shouldallow)
                return;
            
            if (!vehicle.isDrowned || vehicle.lockedOwner == CSteamID.Nil || vehicle.lockedOwner.m_SteamID == 0)
                return;
            
            if (Plugin.Inst.BusyVehicle.Contains(vehicle.instanceID))
                return;
            
            var rPlayer = new RocketPlayer(vehicle.lockedOwner.ToString());
            if (!rPlayer.HasPermission(Plugin.Conf.AutoAddOnDrownPermission) ||
                GarageManager.Count(vehicle.lockedOwner.m_SteamID) >= rPlayer.GetGarageSlot()) 
                return;
            
            if (BlacklistUtil.IsVehicleBlacklisted(vehicle, rPlayer.HasPermission))
            {
                ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName, vehicle.asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            var blacklisted = BlacklistUtil.FindViolation(vehicle, rPlayer.HasPermission);
            if (blacklisted != null)
            {
                var hit = blacklisted.Value;
                ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich((hit.IsBarricade ? EResponse.BLACKLIST_BARRICADE : EResponse.BLACKLIST_ITEM).ToString(), hit.Asset.itemName, hit.Asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            var garageContent = VehicleWrapper.Create(vehicle);
            VehicleUtil.ClearTrunkAndBarricades(vehicle);
            DatabaseManager.Queue.Enqueue(async () => await GarageManager.AddAsync(new PlayerGarage
            {
                VehicleName = vehicle.asset.vehicleName,
                SteamId = vehicle.lockedOwner.m_SteamID,
                GarageContent = garageContent,
                LastUpdated = DateTime.Now
            }));
            if (PlayerTool.getPlayer(vehicle.lockedOwner) != null)
                ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich(EResponse.VEHICLE_DROWN.ToString(), vehicle.asset.vehicleName),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
        }
    }
}
