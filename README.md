# COM3D2.ShapekeyMaster
A ShapeAnimator replacement coded for BepinEx with extended functionality and a sharp focus on performance.

# Overview

ShapekeyMaster AKA SM, is a shapekey animation/setting function to compliment the amazing shapekeys whose potential remains currently untapped in the community. SM's primary aim was making shapekeys actually useful for character creation in a way that didn't interfere with user immersion.

- Performance friendly
- BepinEx
- Shapekey conditionals for clothing and specific menu items
- Completely automatic and persistent
- Some function customization
- Import/Export Functions

## Performance Friendly
I make this claim after timing the functions and what time it took them to complete on average. I've seen numbers as low as 50 Microseconds with rare one frame increases into 1-10 MS which is considered slow.

To put into perspective why these numbers matter, we need to consider the frame time of a game running at 60 FPS. I've extrapolated that a game running at 60FPS needs to put out a new frame every 16.66666~ Miliseconds in order to keep that frame rate. Any time we put a load on our games, we can suffer an FPS hit because processing times take longer so the frame can not be put out as fast. SM's average impact is about 500 microseconds (this is a rounded guess with a ton of key sets and the UI closed). Which has a very minor if not insignificant impact on your FPS.

## Shapekey Conditionals
This is a function that I think shapekeys needed very much to become useful. Shapekey conditionals allow your shapekeys to automatically be enabled/disabled/modified in response to a clothing item or .menu file being active on the applied maid. This allows you to prevent clipping or incompatibility with clothing while still retaining the keys when those conditions don't apply anymore seamlessly.

## Automatic and Persistent
SM basically gatekeeps the shapekey setting function and ensures that the values being set are in line with what it demands. In this way, whenever the game sets keys, it sets our own keys meaning the changes are practically seamless. You won't notice a delay and you'll never have to manually set keys again yourself.

## Function Customization
This is basic support for increasing max deform limits, turning off autosaves, hotkeys which are optional (we use the gear menu) and changing how many entries are displayed per page.

## Import/Export
Since we understand that users may seek to share their maids or keys with other users, SM allows you to easily and convienently export certain shapekeys which can then be shared and given to another user all with the push of a button. Importing for them is a matter of pressing the button, selecting the file and done.

# Usage
1. Download the DLL
2. Download the `COM3D2.API.dll` from https://github.com/DeathWeasel1337/COM3D2_Plugins/releases
3. Place both DLLs into your `BepinEx/plugins` folder.
4. Profit! Simply open the UI with the icon in the gear menu or configure the hotkey in the config (you can use ConfigurationManager, F1 if ConfigurationManager is installed to configure the hotkey and other plugin settings too).

# Convert from SA
We understand a lot of users have ShapekeyAnimator and have used it over a long time and luckily a converter has been created by Pain Brioche and can be found here: https://github.com/Pain-Brioche/ShapekeyMasterConverter

Furthermore, you can refer to this Picture guide by Dybug for more information on Shapekey conversion if you are having trouble, the conversion process has some quirks so it's recommended you give this a look: https://drive.google.com/drive/folders/1UZAPurIqWPRm4tYSXeJI8AychjA6jo7N
