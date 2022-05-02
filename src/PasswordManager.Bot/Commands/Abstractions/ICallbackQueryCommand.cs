using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands.Abstractions; 

public interface ICallbackQueryCommand : IBotCommand {
	Task ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser);
}