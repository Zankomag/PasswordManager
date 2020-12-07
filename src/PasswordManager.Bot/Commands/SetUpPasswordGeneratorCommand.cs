using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using Telegram.Bot.Types.ReplyMarkups;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Extensions;
using System;
using System.Text;
using Passwords;
using Telegram.Bot.Types.Enums;
using System.Linq;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {

	public class SetUpPasswordGeneratorCommand : IMessageCommand, ICallBackQueryCommand {
		public async Task ExecuteAsync(Message message, BotUser user) {
			if (user.Action != UserAction.UpdatePasswordLength) {
				await SendGeneratorSettings(user, message);
			} else {
				int length;
				try {
					length = Convert.ToInt32(message.Text);
					if(length < PasswordManagerService.MinPasswordLength || length > PasswordManagerService.MaxPasswordLength) {
						await BotService.Instance.Client.SendTextMessageAsync(message.From.Id,
							"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
							PasswordManagerService.MinPasswordLength, PasswordManagerService.MaxPasswordLength));
						return;
					}
					user.GenPattern = user.GenPattern.Substring(0, 6) + length.ToString();
					PasswordManagerService.SetUserPasswordPattern(user, user.GenPattern);
					PasswordManagerService.SetUserAction(user, UserAction.Search);
					await SendGeneratorSettings(user, message);
				}
				catch(Exception) {
					await BotService.Instance.Client.SendTextMessageAsync(message.From.Id,
						"⛓️ " + String.Format(Localization.GetMessage("WrongLength", user.Lang),
						PasswordManagerService.MinPasswordLength, PasswordManagerService.MaxPasswordLength));
				}
			}
			
		}

		async Task SendGeneratorSettings(User user, Message message) {
			string messageText = GetMessageText(ref user, message.Text);

			InlineKeyboardMarkup keyboard = GetGeneratorSettingsKeyboard(user);

			await BotService.Instance.Client.SendTextMessageAsync(message.From.Id, messageText, replyMarkup: keyboard, parseMode: ParseMode.Markdown);
		}

		string GetMessageText(ref User user, string messageText) {
			string password = null;
			try {
				string oldPassword = messageText.Split('\n').Last();
				password = user.GenPattern.GeneratePasswordByPattern();
				//TODO:
				//WTF????? DELETE ALL THIS SHIT
				if (oldPassword == password)
				{
					//TODO:
					//WTF?????
					int attempts = 4;
					while (attempts > 0)
					{
						attempts--;
						password = user.GenPattern.GeneratePasswordByPattern();
						
						if (oldPassword != password)
							break;
					}
					//TODO:
					//WTF?????
					if (oldPassword == password)
					{
						PasswordManagerService.SetUserPasswordPattern(user, Password.DefaultPasswordGeneratorPattern);
						password = user.GenPattern.GeneratePasswordByPattern();
						if(oldPassword != password)
						{
							password = DateTime.UtcNow.Ticks.ToString();
						}
					}
				}
			}
			catch (ArgumentException) {
				PasswordManagerService.SetUserPasswordPattern(user);
				password = Password.GeneratePasswordByPattern(Password.DefaultPasswordGeneratorPattern);
				user.GenPattern = Password.DefaultPasswordGeneratorPattern;
			}
			return "🛠 " + string.Format(Localization.GetMessage("SetUpPassword", user.Lang),
				"\n\n`" + password + "`");
		}

		InlineKeyboardMarkup GetGeneratorSettingsKeyboard(User user) {
			if (user.GenPattern == null || user.GenPattern.Length < 7)
				throw new ArgumentException("Generator pattern must contain all  6 params and length");
			bool containsLowerChars = user.GenPattern[0] != '0',
				containsUpperChars = user.GenPattern[1] != '0',
				containsDigits = user.GenPattern[2] != '0',
				containsSpecialChars = user.GenPattern[3] != '0',
				firstCharIsLetter = user.GenPattern[4] != '0',
				containsSpace = user.GenPattern[5] != '0';


			return new InlineKeyboardMarkup(
				new[] {
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsLowerChars.ToEmojiString(true) +
							Localization.GetMessage("LowerChars", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsLowerChars.ToStringCode() +
							containsLowerChars.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsUpperChars.ToEmojiString(true) +
							Localization.GetMessage("UpperChars", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsUpperChars.ToStringCode() +
							containsUpperChars.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsDigits.ToEmojiString(true) +
							Localization.GetMessage("Digits", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsDigits.ToStringCode() +
							containsDigits.ToReverseZeroOneString()
						),
						InlineKeyboardButton.WithCallbackData(
							containsSpace.ToEmojiString(true) +
							Localization.GetMessage("Spaces", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsSpace.ToStringCode() +
							containsSpace.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							firstCharIsLetter.ToEmojiString(true) +
							Localization.GetMessage("FirstChar", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.FirstCharIsLetter.ToStringCode() +
							firstCharIsLetter.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsSpecialChars.ToEmojiString(true) +
							Localization.GetMessage("SpecialChars", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsSpecialChars.ToStringCode() +
							containsSpecialChars.ToReverseZeroOneString()
						),
						InlineKeyboardButton.WithCallbackData(
							"⛓️ " +
							Localization.GetMessage("Length", user.Lang) + " " + user.GenPattern.Substring(6),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.Length.ToStringCode()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							"🌋 " + Localization.GetMessage("Generate", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.Generate.ToStringCode()
						)
					}
				}
			);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await BotService.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			StringBuilder sb = new StringBuilder(user.GenPattern.Substring(0, 6));
			if((SetUpPasswordCommandCode)callbackQuery.Data[1] != SetUpPasswordCommandCode.Length &&
				(SetUpPasswordCommandCode)callbackQuery.Data[1] != SetUpPasswordCommandCode.Generate &&
				callbackQuery.Data[2] == '0') {

				string genString = sb.ToString().Remove(4, 1);
				if (genString.Count(x => x == '1') <= 1)
					return;
			}
			switch ((SetUpPasswordCommandCode)callbackQuery.Data[1]) {
				case SetUpPasswordCommandCode.ContainsLowerChars:
					sb[0] = callbackQuery.Data[2];
					if(sb[0] == '0' && sb[1] == '0')
						sb[4] = '0';
				break;

				case SetUpPasswordCommandCode.ContainsUpperChars:
					sb[1] = callbackQuery.Data[2];
					if (sb[0] == '0' && sb[1] == '0')
						sb[4] = '0';
				break;

				case SetUpPasswordCommandCode.ContainsDigits:
					sb[2] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.FirstCharIsLetter:
					if (callbackQuery.Data[2] == '0' || (callbackQuery.Data[2] != '0' && (sb[0] != '0' || sb[1] != '0')))
						sb[4] = callbackQuery.Data[2];
					else
						return;
				break;

				case SetUpPasswordCommandCode.ContainsSpecialChars:
					sb[3] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.ContainsSpace:
					sb[5] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.Length:
					await BotService.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
						"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
						PasswordManagerService.MinPasswordLength, PasswordManagerService.MaxPasswordLength));
					PasswordManagerService.SetUserAction(user, UserAction.UpdatePasswordLength);
				return;
			}

			user.GenPattern = sb.ToString() + user.GenPattern.Substring(6);
			PasswordManagerService.SetUserPasswordPattern(user, user.GenPattern);
			string messageText = GetMessageText(ref user, callbackQuery.Message.Text);
			
			var keyboard = GetGeneratorSettingsKeyboard(user);
			await BotService.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId, messageText, replyMarkup: keyboard, parseMode: ParseMode.Markdown);

		}

	}
}
