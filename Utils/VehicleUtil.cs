using System;
using System.Collections.Generic;
using System.Linq;
using CCRPGarage.Models;
using SDG.Unturned;
#pragma warning disable 618

namespace CCRPGarage.Utils
{
    internal static class VehicleUtil
    {
        internal static void ClearTrunkAndBarricades(InteractableVehicle vehicle)
        {
            if (vehicle == null)
                return;

            vehicle.trunkItems?.clear();

            var region = BarricadeManager.findRegionFromVehicle(vehicle);
            if (region == null)
                return;

            // The plant is identified by the parent (vehicle) transform, not the barricade.
            if (!BarricadeManager.tryGetPlant(vehicle.transform, out var x, out var y, out var plant, out _))
                return;

            foreach (var drop in region.drops.ToList())
            {
                if (drop == null)
                    continue;

                if (drop.interactable is InteractableStorage storage)
                    storage.items.clear();

                BarricadeManager.destroyBarricade(drop, x, y, plant);
            }
        }
        
        internal static void ClearItems(InteractableVehicle vehicle)
        {
            if (vehicle == null)
                return;

            var region = BarricadeManager.findRegionFromVehicle(vehicle);
            if (region == null)
                return;
            
            foreach (var drop in region.drops)
            {
                if (drop == null)
                    continue;

                if (drop.interactable is InteractableStorage storage)
                    storage.items.clear();
            }
        }

        // Garage numbers are 1-based positions in the player's garage ordered by id, matching the /garages listing.
        internal static List<PlayerGarage> Numbered(IEnumerable<PlayerGarage> garages)
        {
            return garages?.OrderBy(x => x.Id).ToList() ?? new List<PlayerGarage>();
        }

        internal static PlayerGarage FindGarage(List<PlayerGarage> numbered, string query)
        {
            if (numbered == null || numbered.Count == 0 || string.IsNullOrWhiteSpace(query))
                return null;

            query = query.Trim();
            if (int.TryParse(query, out var number))
                return number >= 1 && number <= numbered.Count ? numbered[number - 1] : null;

            return numbered.FirstOrDefault(x => x.VehicleName.Equals(query, StringComparison.OrdinalIgnoreCase)) ??
                   numbered.FirstOrDefault(x => x.VehicleName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static string TranslateRich(string s, params object[] objects)
        {
            var translated = Plugin.Inst?.Translate(s, objects) ?? s;
            return translated.Replace("-=", "<").Replace("=-", ">");
        }
    }
}