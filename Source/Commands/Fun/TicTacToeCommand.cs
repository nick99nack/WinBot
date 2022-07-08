// NOTE: 95% of this code was generated by GitHub CoPilot as an experiment to see what it can do,
// so some parts of it may not be the best way to do things. (In fact a lot aren't)
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using WinBot.Commands.Attributes;

using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;

namespace WinBot.Commands.Main
{
    public class TicTacToe : BaseCommandModule
    {
        [Command("TicTacToe")]
        [Aliases("ttt")]
        [Description("Play a game of Tic Tac Toe with your friends | Code generated by CoPilot")]
        [Usage("[user]")]
        [Category(Category.Fun)]
        public async Task Game(CommandContext Context, DiscordUser user = null)
        {
            if(user == null)
                user = Context.User;
            if(user.Id == Context.User.Id)
            {
                await Context.ReplyAsync("You can't play Tic Tac Toe with yourself, you sad, sad person!");
                return;
            }
            if(user.Id == Context.Client.CurrentUser.Id)
            {
                await Context.ReplyAsync("I can't play Tic Tac Toe with myself!");
                return;
            }
            
            // Create a new game
            TicTacToeGame game = new TicTacToeGame(Context.User, user, Context);

            // Play the game
            await game.Play();

            // If the game was a draw, tell both users they are donkeys
            if(game.isDraw) {
                await Context.ReplyAsync($"{Context.User.Mention} and {user.Mention} are donkeys!");
                return;
            }

            // End of game messages
            await Context.ReplyAsync($"{game.winner.Mention} won the game and {game.loser.Mention} is a donkey!");
            await Context.ReplyAsync($"{game.winner.Mention} and {game.loser.Mention } can go fuck themselves now!");
        }
    }

    class TicTacToeGame {
        public DiscordUser winner, loser;
        public DiscordUser player1, player2;
        public CommandContext Context;
        public bool isRunning = true;
        public bool isDraw = false;

        public TicTacToeGame(DiscordUser player1, DiscordUser player2, CommandContext Context) {
            this.player1 = player1;
            this.player2 = player2;
            this.Context = Context;
        }

        public async Task Play() {
            // Create the grid
            string[,] grid = new string[3,3];
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 3; j++) {
                    grid[i,j] = "-";
                }
            }

            // Create the embed
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Gold);
            embed.WithFooter($"{player1.Username} vs {player2.Username}");
            embed.Title = "Tic Tac Toe";
            embed.Description = "```\n  1 2 3 \n";
            for(int i = 0; i < 3; i++) {
                embed.Description += $"{i+1} ";
                for(int j = 0; j < 3; j++) {
                    embed.Description += $"{grid[i,j]} ";
                }
                embed.Description += "\n";
            }
            embed.Description += "```";

            // Send the embed to the channel
            DiscordMessage message = await Context.Channel.SendMessageAsync(embed: embed);

            // Create the interactivity service
            InteractivityExtension interactivity = Context.Client.GetInteractivity();

            // Create the input prompt message
            DiscordMessage inputMessage = null;

            // Game loop
            while(isRunning) {
                
                // Get player 1's move in a message
                if(inputMessage == null)
                    inputMessage = await Context.Channel.SendMessageAsync($"{player1.Mention}, make your move!");
                else
                    await inputMessage.ModifyAsync(msg => msg.Content = $"{player1.Mention}, make your move!");
                var player1Move = await interactivity.WaitForMessageAsync(x => x.Author.Id == player1.Id && x.Channel.Id == Context.Channel.Id, TimeSpan.FromSeconds(30));
                if(player1Move.Result != null)
                    await player1Move.Result.DeleteAsync();

                // Anti rate limit delay
                await Task.Delay(1000);

                // If we timed out, tell the user and end the game
                if(player1Move.TimedOut) {
                    await Context.ReplyAsync($"{player1.Mention} timed out!");
                    isRunning = false;
                    isDraw = true;
                    return;
                }

                // Validate the move (check if it is two numbers separated by a space)
                string[] move = player1Move.Result.Content.Split(' ');
                if(move.Length != 2) {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move must be two numbers separated by a space!");
                    await Task.Delay(2500);
                    continue;
                }

                // Validate the move (check if it is in the grid)
                int x = -1, y = -1;
                if(!int.TryParse(move[0], out x) || !int.TryParse(move[1], out y)) {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move must be within the grid!");
                    await Task.Delay(2500);
                    continue;
                }

                // Take one off the coordinates to make them 0-indexed
                x--; y--;
                if(x < 0 || x > 2 || y < 0 || y > 2) {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move must be within the grid!");
                    await Task.Delay(2500);
                    continue;
                }

                // Update the grid
                if(grid[y,x] == "-") {
                    grid[y,x] = "X";
                } else {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move that space is already taken!");
                    await Task.Delay(2500);
                    continue;
                }

                // Update the embed
                embed.Description = "```\n  1 2 3 \n";
                for(int i = 0; i < 3; i++) {
                    embed.Description += $"{i+1} ";
                    for(int j = 0; j < 3; j++) {
                        embed.Description += $"{grid[i,j]} ";
                    }
                    embed.Description += "\n";
                }
                embed.Description += "```";
                await message.ModifyAsync(embed: embed.Build());

                // Anti rate limit delay
                await Task.Delay(500);

                // Check if the game is over
                if(CheckForWin(grid)) {
                    isRunning = false;
                    winner = player1;
                    loser = player2;
                    break;
                }

                // Get player 2's move in a message
                player2Retry:
                await inputMessage.ModifyAsync(msg => msg.Content = $"{player2.Mention}, make your move!");
                var player2Move = await interactivity.WaitForMessageAsync(x => x.Author.Id == player2.Id && x.Channel.Id == Context.Channel.Id, TimeSpan.FromSeconds(30));
                if(player2Move.Result != null)
                    await player2Move.Result.DeleteAsync();

                // Anti ratelimit delay
                await Task.Delay(1000);

                // If we timed out, tell the user and end the game
                if(player2Move.TimedOut) {
                    await Context.ReplyAsync($"{player2.Mention} timed out!");
                    isRunning = false;
                    isDraw = true;
                    return;
                }

                // Validate the move (check if it is two numbers separated by a space)
                move = player2Move.Result.Content.Split(' ');
                if(move.Length != 2) {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move must be two numbers separated by a space!");
                    await Task.Delay(2500);
                    goto player2Retry;
                }

                // Validate the move (check if it is in the grid)
                x = -1; y = -1;
                if(!int.TryParse(move[0], out x) || !int.TryParse(move[1], out y)) {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move must be within the grid!");
                    await Task.Delay(2500);
                    goto player2Retry;
                }

                // Take one off the coordinates to make them 0-indexed
                x--; y--;
                if(x < 0 || x > 2 || y < 0 || y > 2) {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move must be within the grid!");
                    await Task.Delay(2500);
                    continue;
                }

                // Update the grid
                if(grid[y,x] == "-") {
                    grid[y,x] = "O";
                } else {
                    await inputMessage.ModifyAsync(msg => msg.Content = $"Invalid move! Move that space is already taken!");
                    await Task.Delay(2500);
                    goto player2Retry;
                }

                // Update the embed
                embed.Description = "```\n  1 2 3 \n";
                for(int i = 0; i < 3; i++) {
                    embed.Description += $"{i+1} ";
                    for(int j = 0; j < 3; j++) {
                        embed.Description += $"{grid[i,j]} ";
                    }
                    embed.Description += "\n";
                }
                embed.Description += "```";
                await message.ModifyAsync(embed: embed.Build());

                // Anti rate limit delay
                await Task.Delay(500);

                // Check if the game is over
                if(CheckForWin(grid)) {
                    isRunning = false;
                    winner = player2;
                    loser = player1;
                    break;
                }
            }
        }

        public bool CheckForWin(string[,] grid) {
            // Check rows
            for(int i = 0; i < 3; i++) {
                if(grid[i,0] == grid[i,1] && grid[i,1] == grid[i,2] && grid[i,0] != "-") {
                    return true;
                }
            }
            
            // Check columns
            for(int i = 0; i < 3; i++) {
                if(grid[0,i] == grid[1,i] && grid[1,i] == grid[2,i] && grid[0,i] != "-") {
                    return true;
                }
            }
            
            // Check diagonals
            if(grid[0,0] == grid[1,1] && grid[1,1] == grid[2,2] && grid[0,0] != "-") {
                return true;
            }
            if(grid[0,2] == grid[1,1] && grid[1,1] == grid[2,0] && grid[0,2] != "-") {
                return true;
            }
            
            // Check for free space
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 3; j++) {
                    if(grid[i,j] == "-") {
                        return false;
                    }
                }
            }
            isDraw = true;
            return true;
        }
    }
}