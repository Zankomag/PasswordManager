namespace PasswordManager.Common.Extensions; 

public static class DatabaseExtensions {
	
	//todo  google wheres should be page count or pages count (on item/items example)
	// and rename accordingly in all methods with "Count" suffix
	public static int PagesCount(this int accountCount, int pageSize) {
		return (int)Math.Ceiling(accountCount / (double)pageSize);
	}

}