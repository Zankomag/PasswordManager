using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Extensions {
	public static class BotExtensions {
		public static int? GetUserIdByUpdateType(this Update update)
			=> update.Type switch {
				UpdateType.Message => update.Message.From.Id,
				UpdateType.CallbackQuery => update.CallbackQuery.From.Id,
				UpdateType.InlineQuery => update.InlineQuery.From.Id,
				UpdateType.ChosenInlineResult => update.ChosenInlineResult.From.Id,
				UpdateType.EditedMessage => update.EditedMessage.From.Id,
				UpdateType.ChannelPost => update.ChannelPost.From.Id,
				UpdateType.EditedChannelPost => update.EditedChannelPost.From.Id,
				UpdateType.Poll => null,
				UpdateType.PollAnswer => update.PollAnswer.User.Id,
				UpdateType.PreCheckoutQuery => update.PreCheckoutQuery.From.Id,
				UpdateType.ShippingQuery => update.ShippingQuery.From.Id,
				UpdateType.Unknown => null,
				_ => throw new ArgumentException("Unknown update type")
			};
	}
}
