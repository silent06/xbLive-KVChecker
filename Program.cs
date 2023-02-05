using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KVChecker_;
using System.Threading;
namespace KvHashHandler
{
    class Program
    {
        /*Notes  ***UseFul SQl queries***    
            Resets Auto_INCREMENT back to the lowest number. 
            ALTER TABLE kvs AUTO_INCREMENT = 1;

            Checks for duplicate hashes
            SELECT * FROM kvs WHERE hash IN ( SELECT hash FROM kvs GROUP BY hash HAVING count(*) > 1) ORDER BY hash;

            Deletes Duplicate hashes
            DELETE S1 FROM kvs AS S1 JOIN kvs AS S2 WHERE S1.id > S2.id AND S1.hash = S2.hash;
        */
        static void Main(string[] args)
        {
            StreamWriter kvlog;
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\MySQLconfig.ini";
            if (!File.Exists(filePath))
            {
                throw new Exception("MySQLconfig file not found");
            }
            string currenttime = string.Format("{0:hh:mm:ss tt}", DateTime.Now.ToUniversalTime().ToLocalTime());
            //File.WriteAllText("KVChecker.log", String.Empty);
            if (!File.Exists("KVChecker.log")) { File.WriteAllText("KVChecker.log", String.Empty); }
            if (!File.Exists("KvRemove.log")) { File.WriteAllText("KvRemove.log", String.Empty); }            
            Utils.LoadedIni = new IniParsing(filePath);
            Global.Database = Utils.GetSqlDatabase();
            Global.host = Utils.GetSqlHostName();
            Global.password = Utils.GetSqlPassword();
            Global.Username = Utils.GetSqlUserName();
            Global.NumberOfKVS = Utils.NumberOfKVS();
            Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), "******************[Running Daily Kv Checker task]******************", "\n" }), ConsoleColor.Green);
            Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " NumberOfKVS: ", Global.NumberOfKVS, "\n" }), ConsoleColor.Green);
            Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " Removing any potential duplicates... ", "\n" }), ConsoleColor.Magenta);
            MySQL.HashDuplicateCheck();
            //byte[] LastHash = new byte[0x4];
            for (int i = 0; i < Global.NumberOfKVS; i++)
            {
                byte[] hashA = new byte[0x4];
                byte[] KVfile = new byte[0];             
                int fileSize = 0;
                ClientInfo info = new ClientInfo();
                MySQL.GetHashData(i.ToString(), ref info);
                Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " Checking...NoKV ID: ", i, " in Folder ",  info.hash, "\n" }), ConsoleColor.Green);
                if (File.Exists("../" + info.hash + "/kv.bin"))
                {

                    KVfile = File.ReadAllBytes("../" + info.hash + "/kv.bin");
                    fileSize = KVfile.Length;
                    Buffer.BlockCopy(KVfile.Skip(0x4).Take(0x4).ToArray(), 0x0, hashA, 0x0, 0x4);

                    //MySQL.CheckHashData(Utils.BytesToString(LastHash), ref info);
                    //if (info.checkhash == Utils.BytesToString(hashA))
                    //{
                        /*Error Check for duplicates*/
                        //Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " Duplicate hash found! Exiting...", "NoKV ID: ", i.ToString(), " in Folder: ", Utils.BytesToString(hashA), "\n" }), ConsoleColor.Red);
                        //Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " removing.... ", "\n" }), ConsoleColor.Magenta);
                        //MySQL.DeleteKV(Utils.BytesToString(hashA));/*Remove KV Hash from sql*/
                        //File.Delete("../" + Utils.BytesToString(hashA) + "/kv.bin");/*Remove Kv.bin from folder*/
                        //Directory.Delete(string.Concat(new object[] { "../", Utils.BytesToString(hashA) }));/*Remove Folder*/
                        //break;
                    //};
                    
                    KVChecker_.KVChecker kvchecker = new KVChecker_.KVChecker();
                    kvchecker.getStatus(KVfile);
                    int num = kvchecker.returnStatus();
                    if (num == 0)
                    {
                        Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " KV Unbanned! Hash: ", Utils.BytesToString(hashA), "\n" }), ConsoleColor.Magenta);
                    }
                    else if (num == 1)
                    {
                        Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " KV banned! Hash: ", Utils.BytesToString(hashA), "\n" }), ConsoleColor.Magenta);
                        Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " removing.... ", "\n" }), ConsoleColor.Magenta);
                        MySQL.DeleteKV(info.hash);/*Remove KV Hash from sql*/
                        File.Delete("../" + info.hash + "/kv.bin");/*Remove Kv.bin from folder*/
                        Directory.Delete(string.Concat(new object[] { "../", info.hash }));/*Remove Folder*/
                        string[] removelog = { currenttime + DateTime.Now.ToLongDateString() + "  Removed KV Hash:  " + info.hash };
                        kvlog = File.AppendText("KvRemove.log");
                        kvlog.WriteLine(string.Concat(new object[] { removelog[0] })); kvlog.Close();
                    }
                    else if (num == 2)
                    {
                        Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), " Error! Hash: ", Utils.BytesToString(hashA), "\n" }), ConsoleColor.Magenta);
                        string[] removelog = { currenttime + DateTime.Now.ToLongDateString() + " Error! Unable to check KV Hash:  " + info.hash };
                        kvlog = File.AppendText("KvRemove.log");
                        kvlog.WriteLine(string.Concat(new object[] { removelog[0] })); kvlog.Close();
                    }
                    Thread.Sleep(5000);
                    //Buffer.BlockCopy(KVfile.Skip(0x4).Take(0x4).ToArray(), 0x0, LastHash, 0x0, 0x4);/*Copy Lasthash for Error Handling*/

                }
                else {
                    Utils.AppendText(string.Concat(new object[] { DateTime.Now.ToLongDateString(), "  :(  ", "hash not found for NoKv ID: ", i.ToString(), "\n" }), ConsoleColor.Red);
                }
            }
            //Console.WriteLine("Done! \n");
            //Console.ReadKey();
        }
    }
}
