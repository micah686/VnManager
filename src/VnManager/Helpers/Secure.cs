using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using AdysTech.CredentialManager;

namespace VnManager.Helpers
{

    
    internal static class Secure
    {

        #region FileEncryption
        /// <summary>
        /// Creates a random salt that will be used to encrypt your file. This method is required on FileEncrypt.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fill the whole array with secure random data 10 times(to make sure it's good and random)
                    rng.GetBytes(data);
                }
            }
            return data;
        }


        private static byte[] Encrypt(byte[] input)
        {
            try
            {
                var cred = CredentialManager.GetCredentials("VnManager.FileEnc");
                if (cred == null) return new byte[0];
                byte[] passwordBytes = Encoding.UTF8.GetBytes(cred.Password);
                byte[] salt = Convert.FromBase64String(cred.UserName);
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 20000);
                Aes aes = new AesManaged();
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                

                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                cs.Close();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "failed to encrypt byte array");
                return new byte[0];
            }
        }

        private static byte[] Decrypt(byte[] input)
        {
            try
            {
                var cred = CredentialManager.GetCredentials("VnManager.FileEnc");
                if (cred == null) return new byte[0];
                byte[] passwordBytes = Encoding.UTF8.GetBytes(cred.Password);
                byte[] salt = Convert.FromBase64String(cred.UserName);
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 20000);
                Aes aes = new AesManaged();
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);


                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                cs.Close();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to decrypt byte array");
                return new byte[0];
            }


        }

        /// <summary>
        /// Encrypts the file at at the specified filepath
        /// </summary>
        /// <param name="path">Path to the file to encrypt. Should not include the .aes extension</param>
        public static void EncFile(string path)
        {
            try
            {
                if(!File.Exists(path))return;
                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length < 20) return;
                byte[] encBytes = Encrypt(bytes);
                if (encBytes == null || encBytes.Length < 20) return;
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
        public static void DecFile(string path)
        {
            try
            {
                string encImagePath = $"{path}.aes";
                byte[] encBytes = File.ReadAllBytes(encImagePath);
                if (encBytes.Length < 20) return;
                byte[] bytes = Decrypt(encBytes);
                if (bytes == null || bytes.Length < 20) return;
                string newPath = $@"{Path.GetDirectoryName(encImagePath)}\{Path.GetFileNameWithoutExtension(encImagePath)}";
                File.WriteAllBytes(newPath, bytes);
                File.Delete(path);
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
        public static void EncStream(MemoryStream stream, string path)
        {
            try
            {
                byte[] bytes = stream.ToArray();
                if (bytes.Length < 20) return;
                byte[] encBytes = Encrypt(bytes);
                if (encBytes == null || encBytes.Length < 20) return;
                File.WriteAllBytes($"{path}.aes", encBytes);
                File.Delete(path);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to encrypt stream");
            }
        }

        /// <summary>
        /// Take a Stream, decrypt it, and write it out to a file
        /// </summary>
        /// <param name="stream">Stream of the object you want to decrypt</param>
        /// <param name="path">Path of where you want the file saved. Should not include the .aes extension</param>
        public static void DecStream(MemoryStream stream, string path)
        {
            try
            {
                string encPath = $"{path}.aes";
                byte[] encBytes = stream.ToArray();
                if (encBytes.Length < 20) return;
                byte[] bytes = Decrypt(encBytes);
                if (bytes == null || bytes.Length < 20) return;
                File.WriteAllBytes(path, bytes);
                File.Delete(encPath);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Failed to decrypt stream");
            }

        }

        #endregion



















        #region Hashing
        //PasswordHash has a 32 byte salt, and a 20 byte hash
        internal static PassHashStruct GenerateHash(SecureString secPassword, byte[] prevSalt)
        {
            try
            {
                byte[] salt = prevSalt;
                var pbkdf2 = new Rfc2898DeriveBytes(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(secPassword)), salt,20000);

                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[52];
                Array.Copy(salt,0, hashBytes,0,32);//copy salt into hashBytes
                Array.Copy(hash,0, hashBytes,32,20);//copy hash into hashBytes

                string savedPasswordHash = Convert.ToBase64String(hashBytes);
                string savedSalt = Convert.ToBase64String(salt);

                var passHash = new PassHashStruct()
                {
                    Salt = savedSalt, Hash = savedPasswordHash
                };
                return passHash;
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Could not generate password hash");
                return new PassHashStruct();
            }

        }

        internal static bool ValidatePassword(SecureString secPassword, string prevSalt, string prevHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(prevHash);

                byte[] salt = Convert.FromBase64String(prevSalt); //32 byte salt, as used from generate salt function
                var pbkdf2 = new Rfc2898DeriveBytes(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(secPassword)), salt, 20000);

                byte[] hash = pbkdf2.GetBytes(20);

                int ok = 1;
                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 32] != hash[i])
                        ok = 0;

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

        public static string GenerateSecurePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
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
                var characterBuffer = new char[length];

                for (var iter = 0; iter < length; iter++)
                {
                    var i = byteBuffer[iter] % 87;

                    if (i < 10)
                    {
                        characterBuffer[iter] = (char)('0' + i);
                    }
                    else if (i < 36)
                    {
                        characterBuffer[iter] = (char)('A' + i - 10);
                    }
                    else if (i < 62)
                    {
                        characterBuffer[iter] = (char)('a' + i - 36);
                    }
                    else
                    {
                        characterBuffer[iter] = Punctuations[i - 62];
                        count++;
                    }
                }

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


        #endregion




        public struct PassHashStruct
        {
            public string Salt;
            public string Hash;
        }
    }
}
