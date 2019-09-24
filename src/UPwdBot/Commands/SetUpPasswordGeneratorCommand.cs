using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Localization.MultiUser;
using Telegram.Bot.Types.ReplyMarkups;
using UPwdBot.Types.Enums;
using UPwdBot.Extensions;
using System;
using System.Text;

namespace UPwdBot.Commands {

	public class SetUpPasswordGeneratorCommand : IMessageCommand, ICallBackQueryCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			string messageText = "🛠 " + Localization.GetMessage("SetUpPassword", user.Lang);

			InlineKeyboardMarkup keyboard = GetGeneratorSettingsKeyboard(user);

			await Bot.Instance.Client.SendTextMessageAsync(message.From.Id, messageText, replyMarkup: keyboard);
			
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
							containsLowerChars.ToZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsUpperChars.ToEmojiString(true) +
							Localization.GetMessage("UpperChars", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsLowerChars.ToStringCode() +
							containsLowerChars.ToZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsDigits.ToEmojiString(true) +
							Localization.GetMessage("Digits", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsDigits.ToStringCode() +
							containsDigits.ToZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsSpace.ToEmojiString(true) +
							Localization.GetMessage("Spaces", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsSpace.ToStringCode() +
							containsSpace.ToZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							containsSpecialChars.ToEmojiString(true) +
							Localization.GetMessage("SpecialChars", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.ContainsSpecialChars.ToStringCode() +
							containsSpecialChars.ToZeroOneString()
						)
					},
					new[] {
						InlineKeyboardButton.WithCallbackData(
							firstCharIsLetter.ToEmojiString(true) +
							Localization.GetMessage("FirstChar", user.Lang),
							CallbackCommandCode.SetUpPasswordGenerator.ToStringCode() +
							SetUpPasswordCommandCode.FirstCharIsLetter.ToStringCode() +
							firstCharIsLetter.ToZeroOneString()
						)
					},
				}
			);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			StringBuilder sb = new StringBuilder(user.GenPattern);
			switch ((SetUpPasswordCommandCode)callbackQuery.Data[1]) {
				case SetUpPasswordCommandCode.ContainsLowerChars:
					sb[0] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.ContainsUpperChars:
					sb[1] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.ContainsDigits:
					sb[2] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.FirstCharIsLetter:
					sb[4] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.ContainsSpecialChars:
					sb[3] = callbackQuery.Data[2];
				break;

				case SetUpPasswordCommandCode.ContainsSpace:
					sb[5] = callbackQuery.Data[2];
				break;
			}

			user.GenPattern = sb.ToString();
			PasswordManager.SetUserPasswordPattern(user, user.GenPattern);
			var keyboard = GetGeneratorSettingsKeyboard(user);
			await Bot.Instance.Client.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId, replyMarkup: keyboard);
		}

	}
}
