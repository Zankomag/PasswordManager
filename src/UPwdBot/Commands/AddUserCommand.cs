using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Localization.MultiUser;
using System;

namespace UPwdBot.Commands {
	public class AddUserCommand: IMessageCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			if(user.Id == Bot.Instance.AdminId.Identifier)
			{
				if (message.Text.Contains(' '))
				{
					try
					{
						string newUserIdStr = message.Text.Split(' ')[1];
						int newUserId = Convert.ToInt32(newUserIdStr);
						PasswordManager.AddUser(newUserId, Localization.defaultLanguage);
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
