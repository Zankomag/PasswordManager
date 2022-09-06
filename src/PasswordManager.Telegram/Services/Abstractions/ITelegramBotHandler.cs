using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Telegram.Services.Abstractions; 

public interface ITelegramBotHandler {
	Task HandleUpdateAsync(Update update);
}