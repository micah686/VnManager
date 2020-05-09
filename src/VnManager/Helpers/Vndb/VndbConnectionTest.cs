using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace VnManager.Helpers.Vndb
{
    public static class VndbConnectionTest
    {
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
