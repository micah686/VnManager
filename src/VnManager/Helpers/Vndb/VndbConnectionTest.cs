// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System;
using System.Net.Sockets;

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
