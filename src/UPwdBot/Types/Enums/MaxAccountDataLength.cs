namespace UPwdBot.Types.Enums {
	public enum MaxAccountDataLength {
		AccountName = 50,
		Link = AccountName + 4, // + ".com"
		Login = 35,
		Password = 2048
	}
}
