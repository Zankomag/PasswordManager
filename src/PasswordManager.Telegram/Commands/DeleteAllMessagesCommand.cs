﻿using PasswordManager.Telegram.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Telegram.Commands; 
//public class DeleteAllMessagesCommand : Abstractions.BotCommand, IMessageCommand {

//	public DeleteAllMessagesCommand(IBot bot) : base(bot) { }
//	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
//		//CYCLE ALL MESSAGES THAT NEEDS TO BE DELETED 
//		try {
//			await bot.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
//		}
//		catch (Telegram.Bot.Exceptions.ApiRequestException) {
//			//
//			//TODO:
//			//
//			//Try to edit:
//			//await Bot.Instance.Client.EditMessageTextAsync(chatId, messageId, "🗑️");
//			//if catch exception than do nothing
//		}
//	}


//}