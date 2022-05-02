namespace PasswordManager.Common.Extensions; 

public static class DatabaseExtensions {
	
	//todo  rename accordingly in all methods with "Count" suffix to "itemCount" instead of "ItemsCount"
	public static int PageCount(this int accountCount, int pageSize) {
		return (int)Math.Ceiling(accountCount / (double)pageSize);
	}

}