using PasswordManager.Bot.Services.Abstractions;
using System.Collections.Generic;

namespace PasswordManager.Bot.Services {
	public class PasswordEncryptionService : IPasswordEncryptionService {
		private readonly Dictionary<long, long> accountIds;

		public PasswordEncryptionService() {
			accountIds = new Dictionary<long, long>();
		}

		public void StartEncryptionRequest(long userId, long accountId)
			=> accountIds[userId] = accountId;

		public void FinishEncryptionRequest(long userId)
			=> accountIds.Remove(userId);

		public long? GetAccountId(long userId) {
			if (accountIds.TryGetValue(userId, out long accountId))
				return accountId;
			return null;
		}
	}
}
