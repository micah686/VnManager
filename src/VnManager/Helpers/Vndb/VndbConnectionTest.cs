using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace VnManager.Helpers.Vndb
{
    public static class VndbConnectionTest
    {
        /// <summary>
        /// Checks to see if the program can connect to the Vndb Api
        /// </summary>
        /// <returns></returns>
        public static bool VndbTcpSocketTest()
        {
            try
            {
                TcpClient client = new TcpClient("api.vndb.org", 19535);
                client.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
