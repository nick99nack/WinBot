# WinWorld-Bot
WinWorld-Bot is a Discord bot designed specifically for use in the [WinWorld Discord server](https://discord.gg/HepVSYH). As of now, it only has rather basic features such as a random dog and cat image command but in the future It'll likely have a leveling system with leaderboards and other more advanced functions like that.

## Usage/Build instructions
1. Clone the repository: ``https://github.com/Starman0620/WinWorld-Bot.git`` and move into the project directory: ``cd WinWorld-Bot/WinWorldBot``
2. Build the source code: ``dotnet build -c Release -r linux-x64`` (If you're building on Windows, the -r option should be win-x64) 
3. Change into the build directory: ``cd bin/Release/netcoreapp3.1/linux-x64/`` (If you're on Windows, you'll once again need to replace linux-x64 with win-x64)
4. Run the bot: ``./WinWorldBot`` or just ``WinWorldBot`` for Windows. This will generate a blank configuration file for you.
5. Edit the ``config.json``  file and add your token into the token field, and an API key for [TheCatAPI](https://thecatapi.com/) if you want the cat command to work.
6. Run the bot once more, as before. Once it has started up (It'll output "Ready" to the terminal), you should be good to go into Discord and use it.
