using System;

namespace Uten.Passwords {
	public static class Password {
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
				// ` characted deleted from list to prevent markdown errors
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

		/// <summary>
		/// Pattern must be 6+ digits.
		/// <para> Each of first 5 digits must be 1 or 0 relatively to <see cref="Generate(uint, bool, bool, bool, bool, bool)" method./></para>
		public static string GeneratePasswordByPattern(this string pattern) {
			if (pattern != null && pattern.Length < 6)
				throw new ArgumentException("PASSWORD PATTERN ERROR");

			string password = Generate(
				Convert.ToUInt32(pattern.Substring(5)),
				pattern[0] == '0' ? false : true,
				pattern[0] == '0' ? false : true,
				pattern[0] == '0' ? false : true,
				pattern[0] == '0' ? false : true,
				pattern[0] == '0' ? false : true);

			return password;
		}
	}
}
