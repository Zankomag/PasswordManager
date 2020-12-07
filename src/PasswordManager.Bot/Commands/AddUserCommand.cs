using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using System;
using PasswordManager.Bot.Commands.Abstractions;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands {
	public class AddUserCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, BotUser user) {
			if(user.Id == BotService.Instance.AdminId.Identifier)
			{
				if (message.Text.Contains(' '))
				{
					try
					{
						string newUserIdStr = message.Text.Split(' ')[1];
						int newUserId = Convert.ToInt32(newUserIdStr);
						PasswordManagerService.AddUser(newUserId, Localization.DefaultLanguageCode);
						await BotService.Instance.Client.SendTextMessageAsync(BotService.Instance.AdminId, "Added new user.\n/userlist");
					}
					catch
					{
						await BotService.Instance.Client.SendTextMessageAsync(BotService.Instance.AdminId, "Invalid User Id");
					}
					
				}
			}
		}
	}
}
