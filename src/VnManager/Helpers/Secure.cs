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
                    // Fill the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        /// <summary>
        /// Encrypts a file from its path and a plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="keyName"></param>
        public static void FileEncrypt(string inputFile, string keyName)
        {

            CryptoStream cs = FileEncryptPrep(inputFile);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];

            try
            {
                int read;
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //MediaTypeNames.Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }
                fsIn.Close();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "FileEncrypt Failed");
            }
            finally
            {
                cs.Close();
            }
        }

        /// <summary>
        /// Encrypts a memory stream to a file on the disk
        /// </summary>
        /// <param name="ms">The MemoryStream</param>
        /// <param name="inputFile">The full filename WITHOUT the .aes extension</param>
        /// <param name="keyName">Name of Key for SecureStore</param>
        public static void FileEncryptStream(MemoryStream ms, string inputFile, string keyName)
        {
            CryptoStream cs = FileEncryptPrep(inputFile);

            //FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];

            try
            {
                int read;
                while ((read = ms.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //MediaTypeNames.Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }
                ms.Close();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "FileEncrypt Failed");
            }
            finally
            {
                cs.Close();
            }
        }



        private static CryptoStream FileEncryptPrep(string inputFile)
        {
            byte[] salt = GenerateRandomSalt();
            FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create);
            var cred = CredentialManager.GetCredentials("VnManager.FileEnc");
            if (cred == null) return null;
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(cred.Password);

            RijndaelManaged AES = new RijndaelManaged()
            {
                KeySize = 256,
                BlockSize = 128,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            };
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            fsCrypt.Write(salt, 0, salt.Length);
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            fsCrypt.Close();
            return cs;
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="keyName"></param>
        public static void FileDecrypt(string inputFile, string keyName)
        {
            var cred = CredentialManager.GetCredentials("VnManager.FileEnc");
            if (cred == null) return;
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(cred.Password);
            byte[] salt = new byte[32];
            var outputFile = Path.GetFileNameWithoutExtension(inputFile);
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            var bytesRead = fsCrypt.Read(salt, 0, salt.Length);
            if (bytesRead < 1) return;
            RijndaelManaged AES = new RijndaelManaged()
            {
                KeySize = 256,
                BlockSize = 128,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            };

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            byte[] buffer = new byte[1048576];
            try
            {
                int read;
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //MediaTypeNames.Application.DoEvents();
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex)
            {
                App.Logger.Error(ex, "FileDecrypt CryptographicException error");
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "FileDecrypt Error");
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "FileDecrypt Error by closing CryptoStream");
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }


        public static MemoryStream FileDecryptStream(string inputFile, string keyName)
        {
            var cred = CredentialManager.GetCredentials("VnManager.FileEnc");
            if (cred == null) return null;
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(cred.Password);
            byte[] salt = new byte[32];
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            var bytesRead = fsCrypt.Read(salt, 0, salt.Length);
            if (bytesRead < 1) return null;
            RijndaelManaged AES = new RijndaelManaged()
            {
                KeySize = 256,
                BlockSize = 128,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            };

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            MemoryStream memoryStream = new MemoryStream();
            byte[] buffer = new byte[1048576];
            try
            {
                int read;
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //MediaTypeNames.Application.DoEvents();
                    memoryStream.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex)
            {
                App.Logger.Error(ex, "FileDecrypt CryptographicException error");
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "FileDecrypt Error");
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "FileDecrypt Error by closing CryptoStream");
            }
            finally
            {
                fsCrypt.Close();
            }
            return memoryStream;
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


        public struct PassHashStruct
        {
            public string Salt;
            public string Hash;
        }
    }
}
