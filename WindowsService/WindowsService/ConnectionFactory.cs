using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WindowsService
{
    public static class ConnectionFactory
    {
        private static SqlConnection _destinationConnection;
        private static SqlConnection _sourceConnection;

        public static SqlConnection GetDestinationConnection
        {
            get
            {
                if (_destinationConnection == null)
                {
                    string destinationConn = ConfigurationManager.ConnectionStrings["DestionationDB"].ConnectionString;
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
                    string sourceConn = ConfigurationManager.ConnectionStrings["SourceDB"].ConnectionString;
                    _sourceConnection = new SqlConnection(sourceConn);
                }
                if (_sourceConnection.State == ConnectionState.Broken || _sourceConnection.State == ConnectionState.Closed)
                    _sourceConnection.Open();
                return _sourceConnection;
            }
        }
    }
}