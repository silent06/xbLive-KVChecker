using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;

namespace KvHashHandler
{
    class MySQL
    {
        public static MySqlConnection Setup()
        {
            return new MySqlConnection(String.Format("Server={0};Port=3306;Database={1};Uid={2};Password={3};", Global.host, Global.Database, Global.Username, Global.password));
        }

        public static bool Connect(MySqlConnection connection)
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException exception)
            {
                Console.WriteLine(exception.Message);
                return false;
            }
        }

        public static void Disconnect(MySqlConnection connection)
        {
            try
            {
                connection.Close();
            }
            catch (MySqlException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public static void AddKVHash(string kvhash)
        {
            using (var db = Setup())
            {
                Connect(db);
                using (var command = db.CreateCommand())
                {
                    command.CommandText = string.Format("INSERT INTO kvs (hash, uses) VALUES (@kv_hash, 0)");
                    command.Parameters.AddWithValue("@kv_hash", kvhash);
                    command.ExecuteNonQuery();
                }
                Disconnect(db);
            }
        }
        public static void DeleteKV(string hash)
        {
            using (var db = Setup())
            {
                Connect(db);
                using (var command = db.CreateCommand())
                {
                    command.CommandText = string.Format("DELETE FROM kvs WHERE `hash` = @hash");
                    command.Parameters.AddWithValue("@hash", hash);
                    command.ExecuteNonQuery();
                }
                Disconnect(db);
            }
        }
        public static bool GetHashData(string id, ref ClientInfo data)
        {
            using (var db = Setup())
            {
                Connect(db);
                using (var command = db.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT * FROM kvs WHERE id = @key");
                    command.Parameters.AddWithValue("@key", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data.hash = reader.GetString("hash");
                            Disconnect(db);
                            return true;
                        }
                    }
                }
                Disconnect(db);
            }
            return false;
        }

        public static void HashDuplicateCheck()
        {
            using (var db = Setup())
            {
                Connect(db);
                using (var command = db.CreateCommand())
                {
                    command.CommandText = string.Format("DELETE S1 FROM kvs AS S1 JOIN kvs AS S2 WHERE S1.id > S2.id AND S1.hash = S2.hash");
                    command.ExecuteNonQuery();
                }
                Disconnect(db);
            }
        }

    }
}
