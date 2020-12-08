using PasswordManager.Bot.Enums;

namespace PasswordManager.Application.Extensions {
	public static class AccountDataTypeExtensions {
		public static MaxAccountDataLength ToMaxAccountDataLength(this AccountDataType value) {
			return value switch {
				AccountDataType.Password => MaxAccountDataLength.Password,
				AccountDataType.AccountName => MaxAccountDataLength.AccountName,
				AccountDataType.Link => MaxAccountDataLength.Link,
				AccountDataType.Login => MaxAccountDataLength.Login,
				_ => throw new System.ArgumentOutOfRangeException(nameof(value))
			};
		}
	}
}
