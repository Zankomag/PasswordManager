using System;
using System.Linq;

namespace Uten.Encryption
{
    public static class Encryption
    {
		private static readonly int offset = 20;
		private static readonly int lowerOffsetBound = 224;
		private static readonly int upperOffsetBound = 383;


		//Used to get strOffset form char
		private static int GetKeyFromChar(char keyChar) {
			int key = 0;
			try {
				key = (int)keyChar - offset;
			} catch(Exception) { }

			return (key >= lowerOffsetBound && key <= upperOffsetBound) ? key : 0;
		}

		/// <summary>
		/// Encrypts a character.
		/// </summary>
		/// <param name="str">character to encrypt</param>
		/// <returns>encrypted string</returns>
		public static string Encrypt(this char str) {
			string encrytptedStr = "";
			Random random = new Random();
			int strOffset = random.Next(lowerOffsetBound, upperOffsetBound);
			try { 
				encrytptedStr += (char)(str + strOffset);
				char key = (char)(strOffset + offset);
				encrytptedStr += key;
			} catch (Exception) {
				encrytptedStr += str;
				encrytptedStr += (char)216;
			}

			return encrytptedStr;
		}

		/// <summary>
		/// Encrypts an array of characters.
		/// </summary>
		/// <param name="str">characters to encrypt</param>
		/// <returns>encrypted string</returns>
		public static string Encrypt(this char[] str) {
			string encrytptedStr = "";
			Random random = new Random();
			int strOffset = random.Next(lowerOffsetBound, upperOffsetBound);
			try {
				for(int i = 0; i< str.Length; i++) {
					encrytptedStr += (char)(str[i] + strOffset);
				}
				
				char key = (char)(strOffset + offset);
				encrytptedStr += key;
			} catch (Exception) {
				encrytptedStr = str.ToString();
				encrytptedStr += (char)216;
			}

			return encrytptedStr;
		}

		/// <summary>
		/// Encrypts a string.
		/// </summary>
		/// <param name="str">string to encrypt</param>
		/// <returns>encrypted string</returns>
		public static string Encrypt(this string str) {
			string encrytptedStr = "";
			Random random = new Random();
			int strOffset = random.Next(lowerOffsetBound, upperOffsetBound);
			try {
				for (int i = 0; i < str.Length; i++) {
					encrytptedStr += (char)(str[i] + strOffset);
				}

				char key = (char)(strOffset + offset);
				encrytptedStr += key;
			} catch (Exception) {
				encrytptedStr = str;
				encrytptedStr += (char)216;
			}

			return encrytptedStr;
		}

		/// <summary>
		/// Decrypts a string.
		/// </summary>
		/// <param name="str">string to decrypt.</param>
		/// <returns>decrypted string.</returns>
		public static string Decrypt(this string str) {
			if (str.Length <= 1) return "String annihilated";

			int strOffset = GetKeyFromChar(str.Last());
			if (strOffset == 0) return "String annihilated";
			else if (strOffset == (216 - 20)) return str.Remove(str.Length - 2);

			string result = "";

			try {
				for(int i = 0; i < str.Length - 1; i++) {
					result += (char)(str[i] - strOffset);
				}
			} catch (Exception) { }

			return result;
		}


	}
}
