using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using System;
using PasswordManager.Bot.Commands.Abstractions;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class AddUserCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, User user) {
			if(user.Id == Bot.Instance.AdminId.Identifier)
			{
				if (message.Text.Contains(' '))
				{
					try
					{
						string newUserIdStr = message.Text.Split(' ')[1];
						int newUserId = Convert.ToInt32(newUserIdStr);
						PasswordManagerHandler.AddUser(newUserId, Localization.defaultLanguage);
						await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "Added new user.\n/userlist");
					}
					catch
					{
						await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "Invalid User Id");
					}
					
				}
			}
		}
	}
}
