using Telegram.Bot.Types;

namespace PasswordManager.Bot.Services.Abstractions {
	public interface IBotHandler {
		void HandleUpdate(Update update);
	}
}
