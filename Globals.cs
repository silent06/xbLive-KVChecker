using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvHashHandler
{
    class Global
    {
        public static string host;
        public static string Username;
        public static string password;
        public static string Database;
        public static short NumberOfKVS;
    }

    public struct ClientInfo
    {
        public int iID;
        public string hash;
    }

}
