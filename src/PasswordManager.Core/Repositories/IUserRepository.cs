using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {
	public interface IUserRepository : IRepository<User>{
		void UpdatePasswordPattern(int userId, string passwordPattern);
		void UpdateLanguage(int userId, string langCode);
		void UpdateAction(int userId, UserAction action);
	}
}
