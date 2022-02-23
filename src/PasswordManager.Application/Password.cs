﻿using System;

namespace Passwords {
	public static class Password {
		public const string DefaultPasswordGeneratorPattern = "11111032";

		public static string Generate(uint length = 32,
			bool containsLowerChars = true,
			bool containsUpperChars = true,
			bool containsDigits = true,
			bool containsSpecialChars = true, 
			bool firstCharIsLetter = true,
			bool containsSpace = false) {


			if (firstCharIsLetter && !containsLowerChars && !containsUpperChars)
				firstCharIsLetter = false;

			string charList = "";
			if (containsLowerChars)
				charList += "abcdefghijklmnopqrstuvwxyz";
			if (containsUpperChars)
				charList += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			if (containsDigits)
				charList += "0123456789";
			if (containsSpace)
				charList += ' ';
			if (containsSpecialChars)
				charList += @"~!@#$%`^&*()_-+={}[]\|:;<>,.?/";
			
			
			//todo in password generator library add custom PasswordGeneratorException for all exception cases
			if (charList.Length == 0) throw new ArgumentException("charList is empty: no generation options selected");

			//TODO use RNGCryptoServiceProvider instead
			Random random = new Random();
			string pwd = "";

			if (firstCharIsLetter) {
				if (containsLowerChars && containsUpperChars)
					pwd += charList[random.Next(0, 52)];
				else
					pwd += charList[random.Next(0, 26)];

				length--;
			}

			for (int i = 0; i < length; i++) {

				pwd += charList[random.Next(0, charList.Length)];
			}

			return pwd;
		}

		/// <summary>
		/// Pattern must be 6+ digits. Example: 11111032.
		/// <para>Each of first 5 digits must be 1 or 0 relatively to <see cref="Generate(uint, bool, bool, bool, bool, bool, bool)"/>.</para>
		/// </summary>
		/// <returns>Generated by pattern password</returns>
		public static string GeneratePasswordByPattern(this string pattern) {
			if (pattern is { Length: < 7 })
				throw new ArgumentException($"Password pattern is wrong: {pattern}");

			string password = Generate(
				Convert.ToUInt32(pattern[6..]),
				pattern[0] != '0',
				pattern[1] != '0',
				pattern[2] != '0',
				pattern[3] != '0',
				pattern[4] != '0',
				pattern[5] != '0');

			return password;
		}
	}
}