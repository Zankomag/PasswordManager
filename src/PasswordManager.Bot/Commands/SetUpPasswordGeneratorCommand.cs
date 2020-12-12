using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using Telegram.Bot.Types.ReplyMarkups;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using System;
using System.Text;
using Passwords;
using Telegram.Bot.Types.Enums;
using System.Linq;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {

	public class SetUpPasswordGeneratorCommand : Abstractions.BotCommand, IMessageCommand, IActionCommand, ICallbackQueryCommand {
		private readonly IUserService userService;

		public SetUpPasswordGeneratorCommand(IBot bot, IUserService userService) : base(bot) {
			this.userService = userService;
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) {
			if (user.Action != UserAction.UpdatePasswordLength) {
				await SendGeneratorSettings(user, message);
			} else {
				int length;
				try {
					length = Convert.ToInt32(message.Text);
					if(length < .MinPasswordLength || length > .MaxPasswordLength) {
						await bot.Client.SendTextMessageAsync(message.From.Id,
							"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
							.MinPasswordLength, .MaxPasswordLength));
						return;
					}
					user.GenPattern = user.GenPattern.Substring(0, 6) + length.ToString();
					.SetUserPasswordPattern(user, user.GenPattern);
					.SetUserAction(user, UserAction.Search);
					await SendGeneratorSettings(user, message);
				}
				catch(Exception) {
					await bot.Client.SendTextMessageAsync(message.From.Id,
						"⛓️ " + String.Format(Localization.GetMessage("WrongLength", user.Lang),
						.MinPasswordLength, .MaxPasswordLength));
				}
			}
			
		}

		async Task SendGeneratorSettings(User user, Message message) {
			string messageText = GetMessageText(ref user, message.Text);

			InlineKeyboardMarkup keyboard = GetGeneratorSettingsKeyboard(user);

			await bot.Client.SendTextMessageAsync(message.From.Id, messageText, replyMarkup: keyboard, parseMode: ParseMode.Markdown);
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
						.SetUserPasswordPattern(user, Password.DefaultPasswordGeneratorPattern);
						password = user.GenPattern.GeneratePasswordByPattern();
						if(oldPassword != password)
						{
							password = DateTime.UtcNow.Ticks.ToString();
						}
					}
				}
			}
			catch (ArgumentException) {
				.SetUserPasswordPattern(user);
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
							SetUpPasswordGeneratorCommandCode.ContainsLowerChars.ToStringCode() +
							containsLowerChars.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsUpperChars.ToEmojiString(true) +
							Localization.GetMessage("UpperChars", user.Lang),
							SetUpPasswordGeneratorCommandCode.ContainsUpperChars.ToStringCode() +
							containsUpperChars.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsDigits.ToEmojiString(true) +
							Localization.GetMessage("Digits", user.Lang),
							SetUpPasswordGeneratorCommandCode.ContainsDigits.ToStringCode() +
							containsDigits.ToReverseZeroOneString()
						),
						InlineKeyboardButton.WithCallbackData(
							containsSpace.ToEmojiString(true) +
							Localization.GetMessage("Spaces", user.Lang),
							SetUpPasswordGeneratorCommandCode.ContainsSpace.ToStringCode() +
							containsSpace.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							firstCharIsLetter.ToEmojiString(true) +
							Localization.GetMessage("FirstChar", user.Lang),
							SetUpPasswordGeneratorCommandCode.FirstCharIsLetter.ToStringCode() +
							firstCharIsLetter.ToReverseZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsSpecialChars.ToEmojiString(true) +
							Localization.GetMessage("SpecialChars", user.Lang),
							SetUpPasswordGeneratorCommandCode.ContainsSpecialChars.ToStringCode() +
							containsSpecialChars.ToReverseZeroOneString()
						),
						InlineKeyboardButton.WithCallbackData(
							"⛓️ " +
							Localization.GetMessage("Length", user.Lang) + " " + user.GenPattern.Substring(6),
							SetUpPasswordGeneratorCommandCode.Length.ToStringCode()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							"🌋 " + Localization.GetMessage("Generate", user.Lang),
							SetUpPasswordGeneratorCommandCode.Generate.ToStringCode()
						)
					}
				}
			);
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			StringBuilder sb = new StringBuilder(user.GenPattern.Substring(0, 6));
			if((SetUpPasswordGeneratorCommandCode)callbackQuery.Data[1] != SetUpPasswordGeneratorCommandCode.Length &&
				(SetUpPasswordGeneratorCommandCode)callbackQuery.Data[1] != SetUpPasswordGeneratorCommandCode.Generate &&
				callbackQuery.Data[2] == '0') {

				string genString = sb.ToString().Remove(4, 1);
				if (genString.Count(x => x == '1') <= 1)
					return;
			}
			switch ((SetUpPasswordGeneratorCommandCode)callbackQuery.Data[1]) {
				case SetUpPasswordGeneratorCommandCode.ContainsLowerChars:
					sb[0] = callbackQuery.Data[2];
					if(sb[0] == '0' && sb[1] == '0')
						sb[4] = '0';
				break;

				case SetUpPasswordGeneratorCommandCode.ContainsUpperChars:
					sb[1] = callbackQuery.Data[2];
					if (sb[0] == '0' && sb[1] == '0')
						sb[4] = '0';
				break;

				case SetUpPasswordGeneratorCommandCode.ContainsDigits:
					sb[2] = callbackQuery.Data[2];
				break;

				case SetUpPasswordGeneratorCommandCode.FirstCharIsLetter:
					if (callbackQuery.Data[2] == '0' || (callbackQuery.Data[2] != '0' && (sb[0] != '0' || sb[1] != '0')))
						sb[4] = callbackQuery.Data[2];
					else
						return;
				break;

				case SetUpPasswordGeneratorCommandCode.ContainsSpecialChars:
					sb[3] = callbackQuery.Data[2];
				break;

				case SetUpPasswordGeneratorCommandCode.ContainsSpace:
					sb[5] = callbackQuery.Data[2];
				break;

				case SetUpPasswordGeneratorCommandCode.Length:
					await bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
						"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
						.MinPasswordLength, .MaxPasswordLength));
					.SetUserAction(user, UserAction.UpdatePasswordLength);
				return;
			}

			user.GenPattern = sb.ToString() + user.GenPattern.Substring(6);
			.SetUserPasswordPattern(user, user.GenPattern);
			string messageText = GetMessageText(ref user, callbackQuery.Message.Text);
			
			var keyboard = GetGeneratorSettingsKeyboard(user);
			await bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId, messageText, replyMarkup: keyboard, parseMode: ParseMode.Markdown);

		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) => _AERROR_ throw new NotImplementedException();
	}
}
