using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NeoSmart.SecureStore;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace VnManager.Helpers
{

    
    internal class Secure
    {
        private readonly string _secretStore = Path.Combine(App.ConfigDirPath, @"secure\secrets.store");
        private readonly string _secretKey = Path.Combine(App.ConfigDirPath, @"secure\secrets.key");




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
        public void FileEncrypt(string inputFile, string keyName)
        {
            byte[] salt = GenerateRandomSalt();
            FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create);
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(ReadSecret(keyName));

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
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Encrypts a memory stream to a file on the disk
        /// </summary>
        /// <param name="ms">The MemoryStream</param>
        /// <param name="inputFile">The full filename WITHOUT the .aes extension</param>
        /// <param name="keyName">Name of Key for SecureStore</param>
        public void FileEncryptStream(MemoryStream ms, string inputFile, string keyName)
        {
            byte[] salt = GenerateRandomSalt();
            FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create);
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(ReadSecret(keyName));

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
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="keyName"></param>
        public void FileDecrypt(string inputFile, string keyName)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(ReadSecret(keyName));
            byte[] salt = new byte[32];
            var outputFile = Path.GetFileNameWithoutExtension(inputFile);
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);
            if (fsCrypt.Length < 1) return;
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


        public MemoryStream FileDecryptStream(string inputFile, string keyName)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(ReadSecret(keyName));
            byte[] salt = new byte[32];
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);
            if (fsCrypt.Length < 1) return null;
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


        #region SecureStore
        internal bool TestSecret(string keyName)
        {
            using (var secMan = SecretsManager.LoadStore(_secretStore))
            {
                secMan.LoadKeyFromFile(_secretKey);
                return secMan.TryGetValue(keyName, out string value);
            }
        }

        internal void SetSecret(string keyName, string secretValue)
        {
            using (var secMan = SecretsManager.LoadStore(_secretStore))
            {
                secMan.LoadKeyFromFile(_secretKey);
                secMan.Set(keyName, secretValue);
                secMan.SaveStore(_secretStore);
            }
        }

        internal string ReadSecret(string keyName)
        {
            using (var secMan = SecretsManager.LoadStore(_secretStore))
            {
                secMan.LoadKeyFromFile(_secretKey);
                return secMan.TryGetValue(keyName, out string value) ? value : "";
            }
        }

        internal void CreateSecureStore()
        {
            if (File.Exists(_secretStore)) return;
            using (var secMan = SecretsManager.CreateStore())
            {
                secMan.LoadKeyFromPassword(GenerateSecureKey(128)); //securely derive key from password
                secMan.Set("FileEnc", GenerateSecureKey(128));

                secMan.ExportKey(_secretKey);
                secMan.SaveStore(_secretStore);
            }

        }

        private static string GenerateSecureKey(int length)
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[length];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        #endregion



        public static bool SecureStringEqual(SecureString secureString1, SecureString secureString2)
        {
            if (secureString1 == null)
            {
                return false;
                //throw new ArgumentNullException("s1");
            }
            if (secureString2 == null)
            {
                return false;
                //throw new ArgumentNullException("s2");
            }

            if (secureString1.Length != secureString2.Length)
            {
                return false;
            }

            IntPtr ssBstr1Ptr = IntPtr.Zero;
            IntPtr ssBstr2Ptr = IntPtr.Zero;

            try
            {
                ssBstr1Ptr = Marshal.SecureStringToBSTR(secureString1);
                ssBstr2Ptr = Marshal.SecureStringToBSTR(secureString2);

                String str1 = Marshal.PtrToStringBSTR(ssBstr1Ptr);
                String str2 = Marshal.PtrToStringBSTR(ssBstr2Ptr);

                return Marshal.PtrToStringBSTR(ssBstr1Ptr).Equals(Marshal.PtrToStringBSTR(ssBstr2Ptr));
            }
            finally
            {
                if (ssBstr1Ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ssBstr1Ptr);
                }

                if (ssBstr2Ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ssBstr2Ptr);
                }
            }
        }

    }
}
