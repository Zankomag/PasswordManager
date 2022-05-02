using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace PasswordManager.Application.Encryption; 

//Source1: https://github.com/2Toad/Rijndael256/issues/13#issuecomment-637724412
//Source2 (doesn't work with 256 blocks): https://stackoverflow.com/a/10177020/11101834
public static class Encryptor {

	/// <summary>
	///     This constant is used to determine the keySize of the encryption algorithm in bits.
	///     Divide this by 8 to get the equivalent number of bytes.
	/// </summary>
	private const int keySize = 256;

		
	//TODO move to appsettings and change to 100000+ (not exactly 100000)
	/// <summary>
	///     This constant determines the number of iterations for the password bytes generation function.
	/// </summary>
	private const int derivationIterations = 40000;

	/// <summary>
	///     Returns AES encrypted string
	/// </summary>
	/// <param name="text"></param>
	/// <param name="key"></param>
	/// <returns>Encrypted string</returns>
	public static string Encrypt(this string text, string key) {
		if(String.IsNullOrEmpty(text))
			throw new ArgumentException("string cannot be null or empty", nameof(text));
		if(String.IsNullOrEmpty(key))
			throw new ArgumentException("string cannot be null or empty", nameof(key));

		// Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
		// so that the same Salt and IV values can be used when decrypting.  
		var saltStringBytes = Generate256BitsOfRandomEntropy();
		var ivStringBytes = Generate256BitsOfRandomEntropy();
		var plainTextBytes = Encoding.UTF8.GetBytes(text);

		using var password = new Rfc2898DeriveBytes(key, saltStringBytes, derivationIterations);
		var keyBytes = password.GetBytes(keySize / 8);
		var engine = new RijndaelEngine(256);
		var blockCipher = new CbcBlockCipher(engine);
		var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
		var keyParam = new KeyParameter(keyBytes);
		var keyParamWithIv = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);

		cipher.Init(true, keyParamWithIv);
		var comparisonBytes = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
		var length = cipher.ProcessBytes(plainTextBytes, comparisonBytes, 0);

		cipher.DoFinal(comparisonBytes, length);
		return Convert.ToBase64String(saltStringBytes.Concat(ivStringBytes).Concat(comparisonBytes).ToArray());
	}

	/// <param name="encryptedText">Encrypted string</param>
	/// <param name="key"></param>
	/// <returns>Decrypted string</returns>
	public static string Decrypt(this string encryptedText, string key) {
		if(String.IsNullOrEmpty(encryptedText))
			throw new ArgumentException("string cannot be null or empty", nameof(encryptedText));
		if(String.IsNullOrEmpty(key))
			throw new ArgumentException("string cannot be null or empty", nameof(key));

		// Get the complete stream of bytes that represent:
		// [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
		var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(encryptedText);

		// Get the saltBytes by extracting the first 32 bytes from the supplied cipherText bytes.
		var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(keySize / 8).ToArray();

		// Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
		var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(keySize / 8).Take(keySize / 8).ToArray();

		// Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
		var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(keySize / 8 * 2)
			.Take(cipherTextBytesWithSaltAndIv.Length - keySize / 8 * 2).ToArray();

		using var password = new Rfc2898DeriveBytes(key, saltStringBytes, derivationIterations);
		var keyBytes = password.GetBytes(keySize / 8);
		var engine = new RijndaelEngine(256);
		var blockCipher = new CbcBlockCipher(engine);
		var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
		var keyParam = new KeyParameter(keyBytes);
		var keyParamWithIv = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);

		cipher.Init(false, keyParamWithIv);
		var comparisonBytes = new byte[cipher.GetOutputSize(cipherTextBytes.Length)];
		var length = cipher.ProcessBytes(cipherTextBytes, comparisonBytes, 0);

		cipher.DoFinal(comparisonBytes, length);

		var nullIndex = comparisonBytes.Length - 1;
		while(comparisonBytes[nullIndex] == 0) {
			nullIndex--;
		}
		comparisonBytes = comparisonBytes.Take(nullIndex + 1).ToArray();


		var result = Encoding.UTF8.GetString(comparisonBytes, 0, comparisonBytes.Length);

		return result;
	}

	private static byte[] Generate256BitsOfRandomEntropy() {
		var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
		using var rngCsp = new RNGCryptoServiceProvider();

		// Fill the array with cryptographically secure random bytes.
		rngCsp.GetBytes(randomBytes);

		return randomBytes;
	}
}