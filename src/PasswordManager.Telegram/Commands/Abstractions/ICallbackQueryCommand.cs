using System.Threading.Tasks;
using PasswordManager.Telegram.Models;
using Telegram.Bot.Types;

namespace PasswordManager.Telegram.Commands.Abstractions; 

public interface ICallbackQueryCommand : IBotCommand {
	Task ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser);
}