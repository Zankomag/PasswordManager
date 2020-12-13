using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Services.Abstractions {
	public interface IBotHandler {
		Task HandleUpdateAsync(Update update);
	}
}
