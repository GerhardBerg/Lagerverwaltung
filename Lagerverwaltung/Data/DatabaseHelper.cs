using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagerverwaltung.Data
{
    public static class DatabaseHelper
    {
        // Verbindung zu deiner SQL Express DB
        private static string connectionString =
            @"Server=LAPTOP-KELLER\SQLEXPRESS;Database=Lagerverwaltung;User Id=sa;Password=Jasmin02#1;";

        // Abfragen, die Daten zurückgeben
        public static DataTable ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        // Abfragen, die nichts zurückgeben (z. B. INSERT, UPDATE, DELETE)
        public static void ExecuteNonQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static T ExecuteScalar<T>(string sql, params SqlParameter[] parameters)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return default;

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
    }
}
