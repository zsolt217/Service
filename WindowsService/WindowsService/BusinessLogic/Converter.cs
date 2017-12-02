using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Timers;

using Topshelf.Logging;
using WindowsService.BusinessLogic;

namespace WindowsService
{
    class Converter
    {
        private bool _testSeed;
        private static readonly LogWriter _log = HostLogger.Get<Converter>();
        Timer _timer;
        private bool _stopping;

        public Converter()
        {
            _testSeed = bool.Parse(ConfigurationManager.AppSettings["Seed"]);
            _timer = new Timer();
        }
        public bool Start()
        {
            _stopping = false;
            _log.InfoFormat($"{DateTime.Now} Service started. ");
            if (_testSeed)
                Seed();

            _timer.Enabled = true;
            _timer.Interval = 30 * 60 * 1000;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            return true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _log.InfoFormat($"{DateTime.Now} Timer elapsed.");
            new MainLogic(ref _stopping);
        }

        public bool Stop()
        {
            _log.InfoFormat($"{DateTime.Now} Shutting down.");
            _stopping = true;
            _timer.Enabled = false;
            _timer = null;
            _log.InfoFormat($"{DateTime.Now} Closing connections.");
            ConnectionFactory.GetSourceConnection.Close();
            ConnectionFactory.GetDestinationConnection.Close();
            _log.InfoFormat($"{DateTime.Now} Connection closed.");
            return true;
        }

        private void Seed()
        {
            try
            {
                if (ConnectionFactory.GetSourceConnection.State != System.Data.ConnectionState.Open)
                    ConnectionFactory.GetSourceConnection.Open();
                _log.InfoFormat($"{DateTime.Now} Already created demo db.");
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
