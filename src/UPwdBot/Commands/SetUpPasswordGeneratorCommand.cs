using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Localization.MultiUser;
using Telegram.Bot.Types.ReplyMarkups;
using UPwdBot.Types.Enums;
using UPwdBot.Extensions;
using System;
using System.Text;
using Uten.Passwords;
using Telegram.Bot.Types.Enums;
using System.Linq;

namespace UPwdBot.Commands {

	public class SetUpPasswordGeneratorCommand : IMessageCommand, ICallBackQueryCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			if (user.ActionType != UserAction.UpdatePasswordLength) {
				await SendGeneratorSettings(user, message);
			} else {
				int length;
				try {
					length = Convert.ToInt32(message.Text);
					if(length < PasswordManager.MinPasswordLength || length > PasswordManager.MaxPasswordLength) {
						await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
							"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
							PasswordManager.MinPasswordLength, PasswordManager.MaxPasswordLength));
						return;
					}
					user.GenPattern = user.GenPattern.Substring(0, 6) + length.ToString();
					PasswordManager.SetUserPasswordPattern(user, user.GenPattern);
					PasswordManager.SetUserAction(user, UserAction.Search);
					await SendGeneratorSettings(user, message);
				}
				catch(Exception) {
					await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
						"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
						PasswordManager.MinPasswordLength, PasswordManager.MaxPasswordLength));
				}
			}
			
		}

		async Task SendGeneratorSettings(Types.User user, Message message) {
			string messageText = GetMessageText(ref user);

			InlineKeyboardMarkup keyboard = GetGeneratorSettingsKeyboard(user);

			await Bot.Instance.Client.SendTextMessageAsync(message.From.Id, messageText, replyMarkup: keyboard, parseMode: ParseMode.Markdown);
		}

		string GetMessageText(ref Types.User user) {
			string password;
			try {
				password = user.GenPattern.GeneratePasswordByPattern();
			}
			catch (ArgumentException) {
				PasswordManager.SetUserPasswordPattern(user);
				password = Password.GeneratePasswordByPattern(Password.defaultPasswordGeneratorPattern);
				user.GenPattern = Password.defaultPasswordGeneratorPattern;
			}
			return "🛠 " + string.Format(Localization.GetMessage("SetUpPassword", user.Lang),
				"\n\n`" + Password.GeneratePasswordByPattern(user.GenPattern) + "`");
		}

		InlineKeyboardMarkup GetGeneratorSettingsKeyboard(Types.User user) {
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
							containsSpecialChars.ToEmojiString(true) +
							Localization.GetMessage("SpecialChars", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsSpecialChars.ToStringCode() +
							containsSpecialChars.ToReverseZeroOneString()
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
							"⛓️ " +
							Localization.GetMessage("Length", user.Lang) + " " + user.GenPattern.Substring(6),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.Length.ToStringCode()
						)
					},
				}
			);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			StringBuilder sb = new StringBuilder(user.GenPattern.Substring(0, 6));
			if((SetUpPasswordCommandCode)callbackQuery.Data[1] != SetUpPasswordCommandCode.Length && callbackQuery.Data[2] == '0') {
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
					await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
						"⛓️ " + String.Format(Localization.GetMessage("EnterLength", user.Lang),
						PasswordManager.MinPasswordLength, PasswordManager.MaxPasswordLength));
					PasswordManager.SetUserAction(user, UserAction.UpdatePasswordLength);
				return;
			}

			user.GenPattern = sb.ToString() + user.GenPattern.Substring(6);
			PasswordManager.SetUserPasswordPattern(user, user.GenPattern);
			string messageText = GetMessageText(ref user);
			var keyboard = GetGeneratorSettingsKeyboard(user);
			await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId, messageText, replyMarkup: keyboard, parseMode: ParseMode.Markdown);

		}

	}
}
