using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Telegram.Commands.Abstractions;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Telegram.Commands; 

public class HelpCommand : Abstractions.BotCommand, IMessageCommand {

	public HelpCommand(IBot bot) : base(bot) { }

	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser)
		//TODO:
		//Create public static method in HelpCommand that returns help message
		=> await Bot.Client.SendTextMessageAsync(message.From.Id, 
			String.Format(Localization.GetMessage("Help", botUser.Lang), 
				"/add", "/all", "/generator", "/language", "/cancel","/help"),
			ParseMode.Markdown);
}