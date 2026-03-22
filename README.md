# MqttTheSpire

Small mod for **Slay the Spire 2** that publishes game events to an MQTT broker.

## What this can do so far
* Sends a small number of events to an existing MQTT broker (like `Mosquitto`).
* Basic connection options for a MQTT broker.
* Customizable MQTT topic prefix.

## What this does not (yet) do
* Support for more complex MQTT configurations.
* Support for automatic Home Assistant sensor discovery.
* Support for lots of more potential events that are available in the StS2 API.

If there's a specific thing you'd like to see tracked, feel free to open an issue.  
Similarly, if there's something preventing you from connecting to a MQTT broker, also open an issue.  
Pull requests are also welcome.

## Installation
TL;DR: Installation is similar to the manual installation of any other mod. 

1. Download the latest release.
2. Extract the archive and copy 'MqttTheSpire' folder to `[STS2-Install-Dir]/mods/`.
3. Edit the `config` file. (See below)
4. Start the game.

If the game shows that the mod loaded with errors, there's a high likelyhood that the connection to the MQTT broker could not be established. 

## Configuration

The mod is configured using the `config` file. 
While the file is a simple JSON file, **do not** change the extension to `.json`.
Otherwise, the game will try to load the `config.json` file as a mod (and fail).

- `Host`: The address of your MQTT broker (default: `localhost`).
- `Port`: The MQTT port (default: 1883).
- `User`: Username for the MQTT broker (optional).
- `Password`: Password for the MQTT broker (optional).
- `Topic`: The base topic for all published messages. (default: `slay_the_spire_2`)

## MQTT Topics

All topics are prefixed with the value of `Topic` from the config (e.g.: `slay_the_spire_2/run/total_floor`).  
**Important**: Topics are not immediately published when the game starts. Starting a run should publish most of these, though.

| Subtopic                | Description                                                   | Example             |
|-------------------------|---------------------------------------------------------------|---------------------|
| `run/start_time`        | Timestamp when the last run started. (`yyyy-MM-dd HH:mm:ss`). | 2026-03-22 16:52:47 |
| `run/ascension_level`   | The current ascension level.                                  | 4                   |
| `run/player/character`  | The character ID being played.                                | IRONCLAD            |
| `run/game_mode`         | Current game mode.                                            | Standard            |
| `run/total_floor`       | Total floors climbed.                                         | 16                  |
| `run/room_type`         | Type of the current room.                                     | Elite               |
| `run/player/gold`       | Current gold amount.                                          | 379                 |
| `run/player/max_hp`     | Maximum HP.                                                   | 72                  |
| `run/player/current_hp` | Current HP.                                                   | 37                  |

## Other stuff

If you liked this mod, also check out **[The Guy Mod](https://steamcommunity.com/sharedfiles/filedetails/?id=3209372643)**, a fun custom character for the original Slay the Spire.

## License

See LICENSE file. (MIT)