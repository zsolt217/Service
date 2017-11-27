using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Topshelf.Logging;

namespace WindowsService
{
    class Converter
    {
        private bool _testSeed;
        private static readonly LogWriter _log = HostLogger.Get<Converter>();
        public Converter()
        {
            //_testSeed = testSeed;
            //Start();
            //Stop();
        }
        public bool Start()
        {
            _log.InfoFormat($"{DateTime.Now} Service started. ");
            //if (_testSeed)
                Seed();
            return true;
        }



        public bool Stop()
        {
            ConnectionFactory.GetSourceConnection.Close();
            ConnectionFactory.GetDestinationConnection.Close();
            return true;
        }

        private void Seed()
        {
            try
            {
                ConnectionFactory.GetSourceConnection.Open();
            }
            catch (Exception ex)//Database not exists
            {
                SqlConnectionStringBuilder bd = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["SourceDB"].ConnectionString);
                bd.InitialCatalog = string.Empty;
                SqlConnection conn = new SqlConnection(bd.ToString());
                _log.InfoFormat($"{DateTime.Now} Created demo db.");
                WriteData(conn);
            }
        }

        private void WriteData(SqlConnection conn)
        {
            string sqlBatch = string.Empty;
            var lines = File.ReadAllLines(@"DemoInit.txt");
            SqlCommand cmd = new SqlCommand(string.Empty, conn);
            conn.Open();
            try
            {
                foreach (string line in lines)
                {
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        cmd.CommandText = sqlBatch;
                        cmd.ExecuteNonQuery();
                        sqlBatch = string.Empty;
                    }
                    else
                    {
                        sqlBatch += line + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat($"{DateTime.Now} Error occured while creating demo DB {ex.Message}");
            }
            finally
            {
                _log.InfoFormat($"{DateTime.Now} Demo db is created.");
                conn.Close();
            }
        }
    }
}
