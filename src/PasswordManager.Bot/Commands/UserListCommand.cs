﻿using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using System;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class UserListCommand: IMessageCommand {
		public async Task ExecuteAsync(Message message, User user) {
			if(user.Id == Bot.Instance.AdminId.Identifier)
			{
				try
				{
					List<User> users = null;
					using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString))
					{
						users = conn.Query<User>("select Id from User").ToList();
					}
					string response = string.Empty;
					for(int i = 0; i < users.Count; i++)
					{
						response += (i + 1) + ". [" + users[i].Id + "](tg://user?id=" + users[i].Id + ") \n\n";
					}
					//TODO:
					//fix @UPwdBot, get bot nickname from bot and save in in bot class
					await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "All @UPwdBot users:\n\n" + response, Telegram.Bot.Types.Enums.ParseMode.Markdown);
				}
				catch(Exception ex)
				{
					await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}