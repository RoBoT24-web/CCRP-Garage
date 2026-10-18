using System;
using System.Collections.Generic;
using System.Linq;
using SDG.Unturned;
using RFRocketLibrary.Utils;

namespace CCRPGarage.Utils
{
    internal readonly struct BlacklistHit
    {
        internal readonly ItemAsset Asset;
        internal readonly bool IsBarricade;

        internal BlacklistHit(ItemAsset asset, bool isBarricade)
        {
            Asset = asset;
            IsBarricade = isBarricade;
        }
    }

    internal static class BlacklistUtil
    {
        internal static bool IsVehicleBlacklisted(InteractableVehicle vehicle, Func<string, bool> hasPermission)
        {
            var list = Plugin.Conf.VehicleBlacklist;
            return list?.IdList != null && !hasPermission(list.BypassPermission) && list.IdList.Contains(vehicle.id);
        }

        // Returns the first barricade or stored item that the player is not allowed to save, or null.
        internal static BlacklistHit? FindViolation(InteractableVehicle vehicle, Func<string, bool> hasPermission)
        {
            var conf = Plugin.Conf;
            var barricadeIds = conf.BarricadeIdBlacklist != null && !hasPermission(conf.BarricadeIdBlacklist.BypassPermission)
                ? conf.BarricadeIdBlacklist.IdList
                : null;
            var barricadeCategories = conf.BarricadeCategoryBlacklist != null &&
                                      !hasPermission(conf.BarricadeCategoryBlacklist.BypassPermission)
                ? conf.BarricadeCategoryBlacklist.CategoryList
                : null;
            var itemIds = conf.ItemIdBlacklist != null && !hasPermission(conf.ItemIdBlacklist.BypassPermission)
                ? conf.ItemIdBlacklist.IdList
                : null;
            var itemCategories = conf.ItemCategoryBlacklist != null &&
                                 !hasPermission(conf.ItemCategoryBlacklist.BypassPermission)
                ? conf.ItemCategoryBlacklist.CategoryList
                : null;

            var region = BarricadeManager.getRegionFromVehicle(vehicle);
            if (region != null)
            {
                foreach (var drop in region.drops)
                {
                    if (drop?.asset == null)
                        continue;

                    if (IsBlocked(drop.asset, new[] {drop.asset.build.ToString()}, barricadeIds, barricadeCategories))
                        return new BlacklistHit(drop.asset, true);

                    if (drop.interactable is not InteractableStorage storage || storage.items == null)
                        continue;

                    var hit = FindItem(storage.items.items, itemIds, itemCategories);
                    if (hit != null)
                        return hit;
                }
            }

            return vehicle.trunkItems != null
                ? FindItem(vehicle.trunkItems.items, itemIds, itemCategories)
                : null;
        }

        private static BlacklistHit? FindItem(IEnumerable<ItemJar> jars, List<ushort> ids, List<string> categories)
        {
            if (jars == null)
                return null;

            foreach (var jar in jars)
            {
                if (jar?.item == null)
                    continue;

                var asset = AssetUtil.GetItemAsset(jar.item.id);
                if (asset != null && IsBlocked(asset, new[] {asset.type.ToString()}, ids, categories))
                    return new BlacklistHit(asset, false);
            }

            return null;
        }

        private static bool IsBlocked(ItemAsset asset, string[] assetCategories, List<ushort> ids, List<string> categories)
        {
            if (ids != null && ids.Contains(asset.id))
                return true;

            return categories != null &&
                   categories.Any(x => assetCategories.Any(c => string.Equals(x?.Trim(), c, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
