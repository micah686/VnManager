using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using AdysTech.CredentialManager;

namespace VnManager.Helpers
{
    /// <summary>
    /// Class used for secure functions (encryption, password generator)
    /// These methods have the DebuggerHidden attribute. This will need to be disabled in order to step into the method
    /// </summary>
    [DebuggerStepThrough]
    internal static class Secure
    {
        private const int SaltBytes = 32;
        private const int HashBytes = 20;
        private const int MinStreamLength = 20;
        #region FileEncryption
        /// <summary>
        /// Creates a random salt that will be used to encrypt your file. This method is required on FileEncrypt.
        /// </summary>
        /// <returns></returns>
        [DebuggerHidden]
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];
            const int maxLoops = 10;
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < maxLoops; i++)
                {
                    // Fill the whole array with secure random data 10 times(to make sure it's good and random)
                    rng.GetBytes(data);
                }
            }
            return data;
        }

        //used from https://stackoverflow.com/questions/60889345/using-the-aesgcm-class/60891115#60891115
        /// <summary>
        /// Encrypt a byte array with AES-GCM
        /// </summary>
        /// <param name="input">Byte array to encrypt</param>
        /// <returns></returns>
        [DebuggerHidden]
        private static byte[] Encrypt(byte[] input)
        {
            const int fourBytes = 4;
            try
            {
                //get encryption key and salt
                var cred = CredentialManager.GetCredentials(App.CredFile);
                if (cred == null)
                {
                    return new byte[0];
                }
                byte[] passwordBytes = Encoding.UTF8.GetBytes(cred.Password);
                byte[] salt = Convert.FromBase64String(cred.UserName);
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 20000).GetBytes(16);
                // Get parameter sizes
                int nonceSize = AesGcm.NonceByteSizes.MaxSize;
                int tagSize = AesGcm.TagByteSizes.MaxSize;
                int cipherSize = input.Length;
                // We write everything into one big array for easier encoding
                int encryptedDataLength = 4 + nonceSize + 4 + tagSize + cipherSize;
                Span<byte> encryptedData = encryptedDataLength < 1024 ? stackalloc byte[encryptedDataLength] : new byte[encryptedDataLength].AsSpan();
                // Copy parameters
                BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(0, fourBytes), nonceSize);
                BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(fourBytes + nonceSize, fourBytes), tagSize);
                var nonce = encryptedData.Slice(4, nonceSize);
                var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
                var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);
                // Generate secure nonce
                RandomNumberGenerator.Fill(nonce);
                // Encrypt
                new AesGcm(key).Encrypt(nonce, input.AsSpan(), cipherBytes, tag);

                return encryptedData.ToArray();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "failed to encrypt byte array");
                return new byte[0];
            }
        }

        /// <summary>
        /// Decrypt an AES-GCM encrypted byte array
        /// </summary>
        /// <param name="input">Bytes to decrypt</param>
        /// <returns></returns>
        [DebuggerHidden]
        private static byte[] Decrypt(byte[] input)
        {
            try
            {
                var cred = CredentialManager.GetCredentials(App.CredFile);
                if (cred == null)
                {
                    return new byte[0];
                }
                byte[] passwordBytes = Encoding.UTF8.GetBytes(cred.Password);
                byte[] salt = Convert.FromBase64String(cred.UserName);
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 20000).GetBytes(16);


                Span<byte> encryptedData = input.AsSpan();
                // Extract parameter sizes
                int nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(0, 4));
                int tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4));
                int cipherSize = encryptedData.Length - 4 - nonceSize - 4 - tagSize;
                // Extract parameters
                var nonce = encryptedData.Slice(4, nonceSize);
                var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
                var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);
                // Decrypt
                Span<byte> plainBytes = cipherSize < 1024 ? stackalloc byte[cipherSize] : new byte[cipherSize];
                new AesGcm(key).Decrypt(nonce, cipherBytes, tag, plainBytes);

                return plainBytes.ToArray();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "failed to decrypt byte array");
                return new byte[0];
            }
        }


        /// <summary>
        /// Encrypts the file at at the specified filepath
        /// </summary>
        /// <param name="path">Path to the file to encrypt. Should not include the .aes extension</param>
        [DebuggerHidden]
        public static void EncFile(string path)
        {
            try
            {
                if(!File.Exists(path))
                {
                    return;
                }
                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length < MinStreamLength)
                {
                    return; //don't encrypt files less than 20 bytes, as that indicates that the file might be bad
                } 
                byte[] encBytes = Encrypt(bytes);
                if (encBytes == null || encBytes.Length < MinStreamLength)
                {
                    return;
                }
                File.WriteAllBytes($"{path}.aes", encBytes);
                File.Delete(path);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to Encrypt File");
            }
        }

        /// <summary>
        /// Encrypts the file at at the specified filepath
        /// </summary>
        /// <param name="path">Path to the file to decrypt. Should not include the .aes extension</param>
        [DebuggerHidden]
        public static void DecFile(string path)
        {
            try
            {
                string encImagePath = $"{path}.aes";
                byte[] encBytes = File.ReadAllBytes(encImagePath);
                if (encBytes.Length < MinStreamLength)
                {
                    return; //don't decrypt files less than 20 bytes, as that indicates that the file might be bad
                } 
                byte[] bytes = Decrypt(encBytes);
                if (bytes == null || bytes.Length < MinStreamLength)
                {
                    return;
                }
                File.WriteAllBytes(path, bytes);
                File.Delete(encImagePath);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to Decrypt file");
            }
        }

        /// <summary>
        /// Take a Stream, encrypt it, and write it out to a file
        /// </summary>
        /// <param name="stream">Stream of the object you want to encrypt</param>
        /// <param name="path">Path of where you want the file saved. Should not include the .aes extension</param>
        [DebuggerHidden]
        public static void EncStream(MemoryStream stream, string path)
        {
            try
            {
                byte[] bytes = stream.ToArray();
                if (bytes.Length < MinStreamLength)
                {
                    return; //don't encrypt streams less than 20 bytes, as that indicates that the file might be bad
                } 
                byte[] encBytes = Encrypt(bytes);
                if (encBytes == null || encBytes.Length < MinStreamLength)
                {
                    return;
                }
                File.WriteAllBytes($"{path}.aes", encBytes);
                File.Delete(path);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to encrypt stream");
            }
        }

        /// <summary>
        /// Take a Stream, decrypt it, and returns an unencrypted stream
        /// </summary>
        /// <param name="stream">Stream of the object you want to decrypt</param>
        [DebuggerHidden]
        public static Stream DecStreamToStream(MemoryStream stream)
        {
            try
            {
                byte[] encBytes = stream.ToArray();
                if (encBytes.Length < MinStreamLength)
                {
                    return null; //don't decrypt streams less than 20 bytes, as that indicates that the file might be bad
                } 
                byte[] bytes = Decrypt(encBytes);
                if (bytes == null || bytes.Length < MinStreamLength)
                {
                    return null;
                }
                var decStream = new MemoryStream(bytes);
                return decStream;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to decrypt stream");
                return null;
            }

        }

        #endregion


        #region Hashing
        /// <summary>
        /// Generates a hash with a password(SecureString), and the bytes of the salt
        /// PasswordHash has a 32 byte salt, and a 20 byte hash
        /// </summary>
        /// <param name="secPassword">Password in a SecureString</param>
        /// <param name="prevSalt">Bytes of the salt</param>
        /// <returns>Returns a PassHash object, which is the Hash and the Salt</returns>
        [DebuggerHidden]
        internal static PassHash GenerateHash(SecureString secPassword, byte[] prevSalt)
        {
            try
            {
                byte[] salt = prevSalt;
                var pbkdf2 = new Rfc2898DeriveBytes(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(secPassword)), salt,20000);

                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[52];
                Array.Copy(salt,0, hashBytes,0,SaltBytes);//copy salt into hashBytes
                Array.Copy(hash,0, hashBytes,SaltBytes,HashBytes);//copy hash into hashBytes

                string savedPasswordHash = Convert.ToBase64String(hashBytes);
                string savedSalt = Convert.ToBase64String(salt);

                var passHash = new PassHash
                {
                    Salt = savedSalt, Hash = savedPasswordHash
                };
                return passHash;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Could not generate password hash");
                return new PassHash();
            }

        }

        /// <summary>
        /// Validates the password against the given hash
        /// </summary>
        /// <param name="secPassword">Password to check</param>
        /// <param name="prevSalt">Base64 string of the salt bytes used with the hash</param>
        /// <param name="prevHash">Base64 string of the hash</param>
        /// <returns></returns>
        [DebuggerHidden]
        internal static bool ValidatePassword(SecureString secPassword, string prevSalt, string prevHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(prevHash);

                byte[] salt = Convert.FromBase64String(prevSalt); //32 byte salt, as used from generate salt function
                var pbkdf2 = new Rfc2898DeriveBytes(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(secPassword)), salt, 20000);

                byte[] hash = pbkdf2.GetBytes(20);

                int ok = 1;
                for (int i = 0; i < HashBytes; i++)
                {
                    if (hashBytes[i + SaltBytes] != hash[i])
                    {
                        ok = 0;
                    }
                }

                return ok == 1;
            }
            catch (Exception ex)
            {
                App.Logger.Warning(ex, "Couldn't validate password");
                return false;

            }
        }

        #endregion

        #region Password Generator
        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        /// <summary>
        /// Generate a random secure password
        /// </summary>
        /// <param name="length">Length of the password. This has to be above 1 or less than 129</param>
        /// <param name="numberOfNonAlphanumericCharacters">Number of special characters in the generated password</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static string GenerateSecurePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            const int maxLength = 128;
            if (length < 1 || length > maxLength)
            {
                throw new ArgumentException(nameof(length));
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException(nameof(numberOfNonAlphanumericCharacters));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);

                var count = 0;

                var characterBuffer = LoopCharBuffer(length, byteBuffer, ref count);

                if (count >= numberOfNonAlphanumericCharacters)
                {
                    return new string(characterBuffer);
                }

                int j;
                var rand = new Random();

                for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
                {
                    int k;
                    do
                    {
                        k = rand.Next(0, length);
                    }
                    while (!char.IsLetterOrDigit(characterBuffer[k]));

                    characterBuffer[k] = Punctuations[rand.Next(0, Punctuations.Length)];
                }

                return new string(characterBuffer);
            }
        }

        /// <summary>
        /// Creates a character buffer of random characters from those allowed
        /// </summary>
        /// <param name="length">Length of the character buffer</param>
        /// <param name="byteBuffer">Byte array of random values</param>
        /// <param name="count">Number of special characters</param>
        /// <returns>Returns the secure password as a char array</returns>
        [DebuggerHidden]
        private static char[] LoopCharBuffer(int length, byte[] byteBuffer, ref int count)
        {
            var characterBuffer = new char[length];

            const int maxNum = 10;
            const int maxCapsAlpha = 36;
            const int maxLowerAlpha = 62;
            
            for (var iter = 0; iter < length; iter++)
            {
                var i = byteBuffer[iter] % 87;

                //Set the Char to the appropriate value
                if (i < maxNum)
                {
                    characterBuffer[iter] = (char)('0' + i);
                }
                else if (i < maxCapsAlpha)
                {
                    characterBuffer[iter] = (char)('A' + i - maxNum);
                }
                else if (i < maxLowerAlpha)
                {
                    characterBuffer[iter] = (char)('a' + i - maxCapsAlpha);
                }
                else
                {
                    characterBuffer[iter] = Punctuations[i - maxLowerAlpha];
                    count++;
                }
            }

            return characterBuffer;
        }

        #endregion


        public class PassHash
        {
            public string Salt { get; set; }
            public string Hash { get; set; }
        }
    }
}
