*A collection of plugins maintained by Primu Pyon*

> [!NOTE]
> These plugins are primarily developed for the PonPon cwls.

## Installation

Add the following to Dalamud Settings > Experimental > Custom Plugin Repositories

`https://raw.githubusercontent.com/priprii/FFXIVPlugins/main/repo.json`

Then search `pyon` in your plugins list to find all the pyon plugins available.

## Plugins

### KtisisPyon

A tool for assisting with Gposing, adapted from [Ktisis](https://github.com/ktisis-tools/Ktisis)v0.3 with additional features:

> [!NOTE]
> Click a feature to display preview/info.

<details><summary>MCDF import (Mare Appearance)</summary><video src="https://github.com/user-attachments/assets/3e9fb3a2-0280-4d57-974f-88e4f8db865d"/></details>
<details><summary>Selective bone import with descendants</summary><video src="https://github.com/user-attachments/assets/8bbaecf2-dd27-4010-a0f4-7b249b4fc3b5"/></details>
<details><summary>Selective bone import for facial expressions</summary><video src="https://github.com/user-attachments/assets/c6fd895b-9ecb-4a7d-bfc0-cc787f8c2a5e"/></details>
<details><summary>Face/Body import options</summary><p><i>Face/Body import options are now in Ktisis v0.3.2, but they're currently buggy so KtisisPyon will continue to implement it differently.</i></p><video src="https://github.com/user-attachments/assets/71cb7439-7a05-4e9d-a64c-16a5b77f87e5"/></details>
<details><summary>Import/export lights</summary><video src="https://github.com/user-attachments/assets/b46153f1-9aba-4cae-9455-61f4c1edee19"/></details>
<details><summary>Pose flipping</summary><p><i>You can also assign a keybind for flipping pose in the Input settings.</i></p><video src="https://github.com/user-attachments/assets/c0abbc44-11d1-44f5-b3a4-75f05469d2e1"/></details>
<details><summary>Hi-res image output</summary><video src="https://github.com/user-attachments/assets/a0327b78-5944-4b85-89b9-98e2049a6ed9"/></details>
<details><summary>Toggle visibility of actor/light objects</summary><p><i>My cursor too shy for video, you toggle visibility with the icon to the left of the actor/light name. :3 The context menu is alternative way too.</i></p><video src="https://github.com/user-attachments/assets/e624210e-8f83-4773-8d1d-4be6ac824211"/></details>
<details><summary>Toggle default visibility of groups/bones in overlay with Pose entity</summary><p><i>When you toggle overlay visibility with the 'Pose' entity, any group/bone you choose to hide will not be made visible. You can still make them visible by specifically toggling their visibility, useful for bones that you rarely or never touch.</i></p><video src="https://github.com/user-attachments/assets/d76fee6a-e46f-456d-8eac-63cc3ecb11d5"/></details>
<details><summary>Skelomae/Nofflebone bone groups</summary><p><i>In addition to IVCS, groups for Skelomae/Nofflebones have been added so you can assign group colours, or hide them with the above visibility feature.</i></p></details>
<details><summary>Camera options (work camera move speed/sensitivity, offset to target pose)</summary><video src="https://github.com/user-attachments/assets/47a637c6-29d4-4aa2-a4ef-df407d9f5af8"/></details>
<details><summary>Gizmo scaling & customization</summary><video src="https://github.com/user-attachments/assets/bb714c4f-31d8-4ae3-aa08-0259ab75aecb"/></details>
<details><summary>Overlay customization & bug fixes</summary><video src="https://github.com/user-attachments/assets/0e8743f4-4fff-44b3-a1a9-173dad6de985"/></details>
<details><summary>Adjust how overlay displays depending on active state of actors</summary><video src="https://github.com/user-attachments/assets/871af91f-922f-4701-b960-0e2ff872486a"/></details>

### PartyPyon

Automatically re-create Party Finder listings. *(To mitigate abuse from bots: This plugin is only for verified venue owners, contact me on Discord for key)*

### PartyRangePyon (Outdated)

Display the distance from party members in the party list.

*Note: This plugin is currently outdated, the functionality of this plugin will soon be moved to TargetPyon.*

### PvPyon (Outdated)

Display names of enemy players in PvP, optionally limited to your friends so you can murder them if you queue in separate parties.

*Note: This plugin is currently outdated, the functionality of this plugin will soon be moved to TargetPyon.*

### PyonCam

Custom camera profiles, similar to [Cammy](https://github.com/UnknownX7/Cammy) with additional features:
- IPC implementation for 'Camera Orbit' integration with TargetPyon
- Simplified UI with preset bug fixes

### SoundPyon

Filter annoying sounds, similar to [SoundFilter](https://git.anna.lgbt/anna/SoundFilter) but updated for Dawntrail & simplified UI.

### TamaPyon (In Development)

A Tamagotchi game that you can play while you play FFXIV.

https://github.com/user-attachments/assets/b0fe778a-4fe5-4051-b1cb-898ddaf95faf

### TargetPyon

Similar to PeepingTom but better, displays names of players in a customizable overlay list who:
- Are currently targeting you
- Previously targeted you (and their current target)
- Targeting those who targeted you

Additional features:
- Customize the font, background & formatting of the overlay list
- Play an audio alert when a player targets you
- Mark players targeting you
- Display direction of players from your facing direction.
- Open Inspect & Adventure Plate windows without needing to target players
- Interactions with listed players (Lock camera orbit, Target, Remove, Adventure Plate, Examine, Locate on map, Toggle visibility, Blacklist)
- Toggle visibility of players who are not in party/friend list

https://github.com/user-attachments/assets/5e9968ad-f95a-44be-a265-0fd4a59d1a75

### TriggerPyon

Conditionally trigger counter/emote/command/title reactions.

Features:
- Trigger counter & emote/text reactions when an instigator either performs a selection of emotes, or sends a message with specific phrases.
- Reaction queueing, for performing multiple reactions in a single trigger with delay/duration
- Conditions for triggering reactions depending on:
  - Who the instigator/receiver are. (Specific player, self, others, all)
  - How the instigator/receiver relate to you. (Eg. Friend/Party/Synced)
  - The status of the instigator/receiver (AFK, Busy, RPing etc)
  - Instigator's distance/angle relative to you
  - Your animated state (Moving/Idle/Sitting/Emoting)
- Display counter as an Honorific title/toast/echo message with custom template per trigger
- Options for LookAt/Target when performing emote reactions.
- Restore specific states when a reaction finishes (Eg. Continue performing a dance emote that the triggered event interrupted)
- Discord activity events (Eg. Display current song you're listening to in Honorific title)
- Preset triggers for easy setup (Hug counter, mimic emotes, spank reaction, discord spotify status)
