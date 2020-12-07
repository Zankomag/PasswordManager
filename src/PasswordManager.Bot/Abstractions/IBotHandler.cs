using Telegram.Bot.Types;

namespace PasswordManager.Bot.Abstractions {
	public interface IBotHandler {
		void HandleUpdate(Update update);
	}
}
