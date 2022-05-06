# OCB Electricity Push Button Circuits - 7 Days to Die (A20) Addon

This mod is not EAC compatible, so you need to turn EAC off!  
Otherwise it should work on linux and also via vortex mod manager.  
Should also work for multiplayer, but hasn't been tested that well yet.

This Mod adds a new block supporting multi-way switching Buttons,
those you often find in hallways, staircases or in living rooms.
Each button in line basically actuates the root button when
pushed. These work similar like regular trigger groups, but
only the root item is actually actuated. These also contribute
to regular trigger groups. You can mix these with other triggers.
Use power relays to break into multiple independent groups.

There is a special mode if you set the power duration to `always`,
or `triggered`, as that option will be auto converted to `always`.
In this mode every push on any button will toggle the root trigger.
This way you can turn lights on and off at any button. You also don't
need to put them in sequence, all belong to the same root trigger.

![Simple Setup](Screens/in-game-simple-setup.jpg)

[![GitHub CI Compile Status][3]][2]

## Download and Install

Simply [download here from GitHub][1] and put into your A20 Mods folder:

- https://github.com/OCB7D2D/ElectricityButtonsPush/archive/master.zip (master branch)

## Developer Notes

It took me some time to figure out how to properly support multiplayer and
this section tries to explain a bit what was involved to get it working.
You can either run a server dedicated or people can join your own game.
In the second case, your own player is basically running on the server,
while every other client needs to get most information from remote.
Powered blocks will have a PowerItem on the server side that is always
loaded and ticked, while on client side this info has to be transmitted
from the server. Furthermore PowerItem can hold additional data, that
can't be easily stored on the client side. Powered Tile Entities use
often a `ClientData` field to hold this information on pure clients.

## Changelog

### Version 0.6.0

- Refactored code for cleaner multiplayer support
- Fixes multiplayer hosted from single player game
- Pushing buttons is now always handled by the server
- Only compatible with Electricity Overhaul >= 0.9.6

### Version 0.5.1

- Fixed description in localization
- Automated deployment and release packaging

### Version 0.5.0

- Some clean-ups and copied bugfix from ElectricityWorkarounds

### Version 0.4.0

- Refactor for A20 compatibility

## Compatibility

I've developed and tested this Mod against version a20.b218.

[1]: https://github.com/OCB7D2D/ElectricityButtonsPush/releases
[2]: https://github.com/OCB7D2D/ElectricityButtonsPush/actions/workflows/ci.yml
[3]: https://github.com/OCB7D2D/ElectricityButtonsPush/actions/workflows/ci.yml/badge.svg
