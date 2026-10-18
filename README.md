# CCRP Garage
A RocketMod / LDM plugin to save/load vehicle to/from a virtual garage. Builds as `CCRP Garage.dll`.

## Commands

| Command | Aliases | Permission | Caller | Description |
|---|---|---|---|---|
| `/garageadd [vehicleName]` | `/gadd`, `/ga` | `garageadd` | Player | Store the vehicle you are in or looking at to your garage. Optionally give it a custom name. |
| `/garages` | `/gg`, `/glist` | `garages` | Player | List the vehicles in your garage with their numbers. |
| `/garageretrieve <vehicleName\|number>` | `/gret`, `/gr` | `garageretrieve` | Player | Retrieve a vehicle from your garage by name or by its number from `/garages`. |
| `/garageadmin <list\|wipe\|del> <player> [id/name]` | `/gadmin` | `garageadmin` | Player / Console | Manage a player's garage (see below). |
| `/garagemigrate <from> <to>` | `/gm` | `garagemigrate` | Player / Console | Migrate the garage database between `mysql`, `litedb` and `json`. |

> Builds compiled with `RELEASEPUNCH` use `/vc` (add), `/vl` (list) and `/vs` (retrieve) as aliases instead.

### Vehicle numbers
Each vehicle in a player's garage is numbered, starting at 1, in the order it was added. The numbers are shown by `/garages` and can be used with `/garageretrieve` (for example `/gr 2`). If a vehicle is removed, the vehicles after it move up one number. Names can be used instead of numbers; an exact name match is preferred over a partial one.

### /gadmin
`<player>` is an online player's name or a SteamID64 (SteamID64 also works for offline players).

| Usage | Description |
|---|---|
| `/gadmin list <player>` | List the player's garage. |
| `/gadmin wipe <player>` | Delete every vehicle in the player's garage. |
| `/gadmin del <player> <id/name>` | Delete one vehicle by number or name. |

## Other permissions

| Permission | Description |
|---|---|
| `garageslot.<slots>` (prefix set by `GarageSlotPermissionPrefix`, e.g. `garageslot.10`) | Garage slot count for the player. |
| `garagedrown` (set by `AutoAddOnDrownPermission`) | Drowned vehicles are added to the owner's garage automatically. |

## Blacklists
Configured in the plugin configuration. Each list has its own `BypassPermission`; players with that permission ignore the list.

| Setting | Matches | Default bypass permission |
|---|---|---|
| `VehicleBlacklist` | Vehicle IDs | `garagebypass.vehicle` |
| `BarricadeIdBlacklist` | Barricade IDs on the vehicle | `garagebypass.barricade.id` |
| `BarricadeCategoryBlacklist` | Barricade types on the vehicle | `garagebypass.barricade.category` |
| `ItemIdBlacklist` | Item IDs in barricade storage and the vehicle trunk | `garagebypass.item.id` |
| `ItemCategoryBlacklist` | Item types in barricade storage and the vehicle trunk | `garagebypass.item.category` |

Category names are Unturned enum names and are case-insensitive. The barricade category default is `TORCH`, `CAMPFIRE`, `SPOT`.

**Barricade categories (`EBuild`):** `FORTIFICATION`, `BARRICADE`, `DOOR`, `GATE`, `BED`, `LADDER`, `STORAGE`, `FARM`, `TORCH`, `CAMPFIRE`, `SPIKE`, `WIRE`, `GENERATOR`, `SPOT`, `SAFEZONE`, `FREEFORM`, `SIGN`, `VEHICLE`, `CLAIM`, `BEACON`, `STORAGE_WALL`, `BARREL_RAIN`, `OIL`, `CAGE`, `SHUTTER`, `TANK`, `CHARGE`, `SENTRY`, `SENTRY_FREEFORM`, `OVEN`, `LIBRARY`, `OXYGENATOR`, `GLASS`, `NOTE`, `HATCH`, `MANNEQUIN`, `STEREO`, `SIGN_WALL`, `CLOCK`

**Item categories (`EItemType`):** `HAT`, `PANTS`, `SHIRT`, `MASK`, `BACKPACK`, `VEST`, `GLASSES`, `GUN`, `SIGHT`, `TACTICAL`, `GRIP`, `BARREL`, `MAGAZINE`, `FOOD`, `WATER`, `MEDICAL`, `MELEE`, `FUEL`, `TOOL`, `BARRICADE`, `STORAGE`, `BEACON`, `FARM`, `TRAP`, `STRUCTURE`, `SUPPLY`, `THROWABLE`, `GROWER`, `OPTIC`, `REFILL`, `FISHER`, `CLOUD`, `MAP`, `KEY`, `BOX`, `ARREST_START`, `ARREST_END`, `TANK`, `GENERATOR`, `DETONATOR`, `CHARGE`, `LIBRARY`, `FILTER`, `SENTRY`, `VEHICLE_REPAIR_TOOL`, `TIRE`, `COMPASS`, `OIL_PUMP`