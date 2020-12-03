using PasswordManager.Types.Enums;

namespace PasswordManager.Extensions {
	public static class AccountDataTypeExtensions {
		public static MaxAccountDataLength ToMaxAccountDataLength(this AccountDataType value) {
			switch (value) {
				case AccountDataType.Password:
				return MaxAccountDataLength.Password;

				case AccountDataType.AccountName:
				return MaxAccountDataLength.AccountName;

				case AccountDataType.Link:
				return MaxAccountDataLength.Link;

				case AccountDataType.Login:
				return MaxAccountDataLength.Login;

				default:
				return default;
			}
		}
	}
}
