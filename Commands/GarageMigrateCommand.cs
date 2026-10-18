using System;
using System.Threading.Tasks;
using CCRPGarage.DatabaseManagers;
using CCRPGarage.Enums;
using CCRPGarage.Utils;
using Rocket.API;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace CCRPGarage.Commands
{
    [CommandAliases("gm")]
    [CommandActor(AllowedCaller.Both)]
    [CommandPermissions("garagemigrate")]
    [CommandInfo("Migrate garage database from one to another.", "/garagemigrate <from: mysql|litedb|json> <to: mysql|litedb|json>", AllowSimultaneousCalls = false)]
    public class GarageMigrateCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var from) ||
                !Enum.TryParse<EDatabase>(context.CommandRawArguments[1], true, out var to))
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (from == to)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.SAME_DATABASE.ToString()),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!GarageManager.Ready)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.DATABASE_NOT_READY.ToString()),
                    CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.MIGRATION_START.ToString(), from, to),
                CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
            await DatabaseManager.Queue.Enqueue(async () => await GarageManager.MigrateAsync(from, to))!;
            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.MIGRATION_FINISH.ToString()),
                CCRPGarage.Plugin.MsgColor, CCRPGarage.Plugin.Conf.MessageIconUrl);
        }
    }
}