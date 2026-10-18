using System;
using System.Linq;
using System.Threading.Tasks;
using CCRPGarage.DatabaseManagers;
using CCRPGarage.Enums;
using CCRPGarage.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace CCRPGarage.Commands
{
    [CommandActor(AllowedCaller.Both)]
    [CommandAliases("gadmin")]
    [CommandPermissions("garageadmin")]
    [CommandInfo("Manage a player's garage.", "/gadmin <list|wipe|del> <player> [id/name]",
        AllowSimultaneousCalls = false)]
    public class GarageAdminCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            var args = context.CommandRawArguments;
            var action = args.Length > 0 ? args[0].ToLower() : string.Empty;
            var valid = action == "list" && args.Length == 2 ||
                        action == "wipe" && args.Length == 2 ||
                        action == "del" && args.Length >= 3;
            if (!valid)
            {
                await ReplyAsync(context, EResponse.INVALID_PARAMETER, Syntax);
                return;
            }

            var steamId = ResolvePlayer(args[1], out var displayName);
            if (steamId == 0)
            {
                await ReplyAsync(context, EResponse.PLAYER_NOT_FOUND);
                return;
            }

            var garages = VehicleUtil.Numbered(await GarageManager.Get(steamId));
            if (garages.Count == 0)
            {
                await ReplyAsync(context, EResponse.GADMIN_EMPTY, displayName);
                return;
            }

            switch (action)
            {
                case "list":
                    await ReplyAsync(context, EResponse.GADMIN_HEADER, displayName, garages.Count);
                    for (var i = 0; i < garages.Count; i++)
                    {
                        var garage = garages[i];
                        await ReplyAsync(context, EResponse.GARAGE_LIST, i + 1, garage.VehicleName,
                            garage.GarageContent.Id, garage.GarageContent.GetVehicleAsset()?.vehicleName);
                    }

                    break;
                case "wipe":
                    foreach (var garage in garages)
                    {
                        var id = garage.Id;
                        await DatabaseManager.Queue.Enqueue(async () => await GarageManager.DeleteAsync(id))!;
                    }

                    await ReplyAsync(context, EResponse.GADMIN_WIPED, displayName, garages.Count);
                    break;
                case "del":
                    var query = string.Join(" ", args.Skip(2));
                    var target = VehicleUtil.FindGarage(garages, query);
                    if (target == null)
                    {
                        await ReplyAsync(context, EResponse.VEHICLE_NOT_FOUND, query);
                        return;
                    }

                    await DatabaseManager.Queue.Enqueue(async () => await GarageManager.DeleteAsync(target.Id))!;
                    await ReplyAsync(context, EResponse.GADMIN_DELETED, displayName, target.VehicleName);
                    break;
            }
        }

        // Accepts an online player name or a SteamID64 (works for offline players).
        private static ulong ResolvePlayer(string input, out string displayName)
        {
            displayName = input;
            var online = UnturnedPlayer.FromName(input);
            if (online != null)
            {
                displayName = online.CharacterName;
                return online.CSteamID.m_SteamID;
            }

            return ulong.TryParse(input, out var steamId) && steamId > 76561190000000000UL ? steamId : 0;
        }

        private static Task ReplyAsync(CommandContext context, EResponse response, params object[] args)
        {
            return context.ReplyAsync(VehicleUtil.TranslateRich(response.ToString(), args), CCRPGarage.Plugin.MsgColor,
                CCRPGarage.Plugin.Conf.MessageIconUrl);
        }
    }
}

