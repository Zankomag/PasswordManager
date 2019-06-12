using System;

namespace Uten.Passwords {
	public class Password {
		public static string Generate(uint length = 32,
			bool containsLowerChars = true,
			bool containsUpperChars = true,
			bool containsDigits = true,
			bool containsSpecialChars = true, 
			bool firstCharIsLetter = true) {

		
			if (firstCharIsLetter && !containsLowerChars ||
				firstCharIsLetter && !containsUpperChars)
				throw new ArgumentNullException("firstCharIsLetter", "Cannot set letter as first character, because it does not exist.");

			string charList = "";
			if (containsLowerChars)
				charList += "abcdefghijklmnopqrstuvwxyz";
			if (containsUpperChars)
				charList += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			if (containsDigits)
				charList += "0123456789";
			if (containsSpecialChars) {
				charList += @"`~!@#$%^&*()_-+={}[]\|:;<>,.?/";
			}

			if (charList.Length == 0) throw new ArgumentNullException("No option selected");

			Random random = new Random();
			string pwd = "";

			if (firstCharIsLetter) {
				pwd += charList[random.Next(0, 53)];
				length--;
			}

			for (int i = 0; i < length; i++)
				pwd += charList[random.Next(0, charList.Length)];

			return pwd;
		}
	}
}
