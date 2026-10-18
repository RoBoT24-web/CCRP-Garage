using System.Collections.Generic;
using CCRPGarage.Enums;
using CCRPGarage.Models;
using Rocket.API;

namespace CCRPGarage
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool Enabled;
        public EDatabase Database;
        public string MySqlConnectionString;
        public string MessageColor;
        public string MessageIconUrl;
        public bool AutoClearDestroyedVehicles;
        public bool AutoAddOnDrown;
        public string AutoAddOnDrownPermission;
        public bool AllowTrain;
        public int DefaultGarageSlot;
        public string GarageSlotPermissionPrefix;
        public Blacklist VehicleBlacklist;
        public Blacklist BarricadeIdBlacklist;
        public CategoryBlacklist BarricadeCategoryBlacklist;
        public Blacklist ItemIdBlacklist;
        public CategoryBlacklist ItemCategoryBlacklist;
        public void LoadDefaults()
        {
            Enabled = true;
            Database = EDatabase.LITEDB;
            MySqlConnectionString = "SERVER=127.0.0.1;DATABASE=unturned;UID=root;PASSWORD=123456;PORT=3306;TABLENAME=rfgarage;";
            MessageColor = "magenta";
            MessageIconUrl = "https://cdn.jsdelivr.net/gh/RiceField-Plugins/UnturnedImages@images/plugin/Announcer.png";
            AutoClearDestroyedVehicles = true;
            AutoAddOnDrown = true;
            AutoAddOnDrownPermission = "garagedrown";
            AllowTrain = false;
            DefaultGarageSlot = 5;
            GarageSlotPermissionPrefix = "garageslot";
            VehicleBlacklist = new Blacklist
            {
                BypassPermission = "garagebypass.vehicle",
                IdList = new List<ushort> {1, 2}
            };
            BarricadeIdBlacklist = new Blacklist
            {
                BypassPermission = "garagebypass.barricade.id",
                IdList = new List<ushort> {1, 2}
            };
            BarricadeCategoryBlacklist = new CategoryBlacklist
            {
                BypassPermission = "garagebypass.barricade.category",
                CategoryList = new List<string> {"TORCH", "CAMPFIRE", "SPOT"}
            };
            ItemIdBlacklist = new Blacklist
            {
                BypassPermission = "garagebypass.item.id",
                IdList = new List<ushort> {1, 2}
            };
            ItemCategoryBlacklist = new CategoryBlacklist
            {
                BypassPermission = "garagebypass.item.category",
                CategoryList = new List<string> {"GUN"}
            };        }
    }
}
