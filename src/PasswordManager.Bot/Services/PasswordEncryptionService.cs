using PasswordManager.Bot.Services.Abstractions;
using System.Collections.Generic;

namespace PasswordManager.Bot.Services {
	public class PasswordEncryptionService : IPasswordEncryptionService {
		private readonly Dictionary<int, long> accountIds;

		public PasswordEncryptionService() {
			accountIds = new Dictionary<int, long>();
		}

		public void StartEncryptionRequest(int userId, long accountId)
			=> accountIds[userId] = accountId;

		public void FinishEncryptionRequest(int userId)
			=> accountIds.Remove(userId);

		public long? GetAccountId(int userId) {
			if (accountIds.TryGetValue(userId, out long accountId))
				return accountId;
			return null;
		}
	}
}
