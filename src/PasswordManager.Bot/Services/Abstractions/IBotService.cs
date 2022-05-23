using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Services.Abstractions;

public interface IBotService : IUpdateHandler {

	Task HandleUpdateAsync(Update update);

	bool IsTokenCorrect(string token);
	
}