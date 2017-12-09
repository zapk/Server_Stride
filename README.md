# Server_Stride
[![GitHub release](https://img.shields.io/github/release/zapk/Server_Stride.svg)]() [![GitHub issues](https://img.shields.io/github/issues/zapk/Server_Stride.svg)](https://github.com/zapk/Server_Stride/issues)

Footsteps and footprints for [Blockland](http://blockland.us/).

## Installation
Download Server_Stride.zip from the [latest release](https://github.com/zapk/Server_Stride/releases) and put it in Blockland\\Add-Ons.

### Disclaimer

**Do not** straight-up download the repository unless you know what you're doing as it's most likely partly or mostly broken.

## Admin Commands
- **/clearFootsteps**
	- clears all Stride material settings
- **/setFootstep [material] [hasFootprints]**
	-	assigns settings to your current selected paint colour
	- *Ex. /setFootstep dirt 1*

## Wrench Events
- **setMaterial**
	-	`[material]` `[hasFootprints]`

## Preferences
- **Default Material**
	-	Default footstep sound name for unassigned colours. Must have sounds in res/steps
- **Default Footprints**
	-	Whether or not unassigned colours should have footprint decals.

## Media
![Footprints](http://i.imgur.com/5zjaSAk.jpg)
![Upclose Footprints](http://i.imgur.com/TlWG7J1.png)

## Credits
- [Zapk](https://github.com/zapk) - Server scripts
- [Port](https://github.com/qoh) - Script_Footsteps base, decal library
