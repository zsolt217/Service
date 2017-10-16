using System.Data;
using System.Data.SqlClient;

namespace WindowsService
{
    internal static class ConnectionFactory
    {
        private static SqlConnection _destinationConnection;
        private static SqlConnection _sourceConnection;

        public static SqlConnection GetDestinationConnection
        {
            get
            {
                if (_destinationConnection == null)
                {
                    string destinationConn = "Data Source = localhost\\SQL2012ST; Initial Catalog = ReportDB; Persist Security Info = True; User ID = admin; Password = admin";
                    _destinationConnection = new SqlConnection(destinationConn);
                }
                if (_destinationConnection.State == ConnectionState.Broken || _destinationConnection.State == ConnectionState.Closed)
                    _destinationConnection.Open();
                return _destinationConnection;
            }
        }

        public static SqlConnection GetSourceConnection
        {
            get
            {
                if (_sourceConnection == null)
                {
                    string sourceConn = " Data Source = localhost\\SQL2012ST; Initial Catalog = kszstart_demo; Persist Security Info = True; User ID = Admin; Password = admin";
                    _sourceConnection = new SqlConnection(sourceConn);
                }
                if (_sourceConnection.State == ConnectionState.Broken || _sourceConnection.State == ConnectionState.Closed)
                    _sourceConnection.Open();
                return _sourceConnection;
            }
        }

        public static void Dispose()
        {
            _sourceConnection.Close();
            _destinationConnection.Close();
        }
    }
}