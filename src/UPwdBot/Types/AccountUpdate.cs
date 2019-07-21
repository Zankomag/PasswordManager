using UPwdBot.Types.Enums;

namespace UPwdBot.Types {
	public class AccountUpdate {
		public AccountUpdate(string accountToUpdateId, int messageToDeleteId1, int messageToDeleteId2, AccountDataType accountDataType) {
			AccountToUpdateId = accountToUpdateId;
			MessagetoDeleteId = new int[2];
			MessagetoDeleteId[0] = messageToDeleteId1;
			MessagetoDeleteId[1] = messageToDeleteId2;
			AccountDataType = accountDataType;
		}
		public string AccountToUpdateId;
		public int[] MessagetoDeleteId;
		public AccountDataType AccountDataType;
	}
}
