using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Topshelf.Logging;

namespace WindowsService.BusinessLogic
{
    class MainLogic
    {
        private SqlCommand _commandGetQueries = new SqlCommand(@"SELECT Id
                                              , NextUpdating
                                              , ResultTableName
                                              , SQL
                                              , TranslatedColumnNames
                                              , Name
                                              , CreationDate
                                              , QueryGUID
                                              , ModifyDate
                                          ,  UpdatePeriodTicks
                                          FROM Query",
           ConnectionFactory.GetDestinationConnection);

        private List<Query> _queries;
        private static readonly LogWriter _log = HostLogger.Get<MainLogic>();

        public MainLogic(ref bool stopping)
        {
            _queries = new List<Query>();
            ReadQueries();
            foreach (var query in _queries)
            {
                if (!stopping && DateTime.Now >= query.NextUpdating)
                {
                    _log.InfoFormat($"{DateTime.Now} Updating {query.Name} source.");
                    ExecuteQuery(query);
                    query.NextUpdating = DateTime.Now + query.UpdatePeriod;
                    UpdateQuery(query);
                }
            }
        }


        private void ExecuteQuery(Query query)
        {
            string tableName;
            if (String.IsNullOrEmpty(query.ResultTableName))
            {
                tableName = "a" + Guid.NewGuid().ToString().Replace("-", String.Empty); //azért fűzök hozzá egy karaktert, mert a GUID kezdődhet számmal is, az pedig nem valid táblanév      
                query.ResultTableName = tableName;
                _log.InfoFormat($"{DateTime.Now} Table name generated, update query.");
                UpdateQuery(query);
                _log.InfoFormat($"{DateTime.Now} Query updated.");
            }
            else
            { tableName = query.ResultTableName; }
            _log.InfoFormat($"{DateTime.Now} Deleting view if exists.");
            new SqlCommand(String.Format(@"if exists(select 1 from sys.views where name='{0}' and type='v')
                                                    drop view {0}", tableName), ConnectionFactory.GetSourceConnection).ExecuteNonQuery();
            _log.InfoFormat($"{DateTime.Now} Drop destination table if exists.");
            new SqlCommand(String.Format(@"if exists(select 1 from sys.tables where name='{0}' and type='U')
                                                    drop table {0}", tableName), ConnectionFactory.GetDestinationConnection).ExecuteNonQuery();
            _log.InfoFormat($"{DateTime.Now} Copy data into view.");
            new SqlCommand(String.Format("create view {0} as {1}", tableName, query.SQL), ConnectionFactory.GetSourceConnection).ExecuteNonQuery();
            new SqlCommand(String.Format(@"select * into[{1}].[dbo].[{0}]
                                                    from {0}", tableName, new SqlConnectionStringBuilder(ConnectionFactory.GetDestinationConnection.ConnectionString).InitialCatalog), ConnectionFactory.GetSourceConnection).ExecuteNonQuery();
            _log.InfoFormat($"{DateTime.Now} Copty data from view to destanation table.");
            //TODO: hálózatosság vizsgálata, plussz primary key írás ha nincs.
        }

        private void ReadQueries()
        {
            _log.InfoFormat($"{DateTime.Now} Reading Queries.");
            using (SqlDataReader dr = _commandGetQueries.ExecuteReader())
            {
                while (dr.Read())
                {
                    Query query = new Query
                    {
                        Id = int.Parse(dr["Id"].ToString()),
                        NextUpdating = DateTime.Parse(dr["NextUpdating"].ToString()),
                        ResultTableName = dr["ResultTableName"].ToString(),
                        SQL = dr["SQL"].ToString(),
                        TranslatedColumnNames = dr["TranslatedColumnNames"].ToString(),
                        UpdatePeriodTicks = long.Parse(dr["UpdatePeriodTicks"].ToString()),
                        Name = dr["Name"].ToString()
                    };
                    _queries.Add(query);
                }
            }
            _log.InfoFormat($"{DateTime.Now} Reading queries is finished.");
        }

        private void UpdateQuery(Query query)
        {
            SqlCommand updateQuery = new SqlCommand(String.Format(@"update Query set [NextUpdating] = @NextUpdating
                                          ,[ResultTableName] = @ResultTableName
                                          ,[SQL] = @SQL
                                          ,[TranslatedColumnNames] = @TranslatedColumnNames
                                          ,[UpdatePeriodTicks] = @UpdatePeriodTicks
                                          ,[Name] = @Name
                                          where Id = {0}", query.Id), ConnectionFactory.GetDestinationConnection);
            updateQuery.Parameters.Add("@NextUpdating", SqlDbType.DateTime2).Value = query.NextUpdating;
            updateQuery.Parameters.Add("@ResultTableName", SqlDbType.NVarChar).Value = query.ResultTableName;
            updateQuery.Parameters.Add("@SQL", SqlDbType.NVarChar).Value = query.SQL;
            updateQuery.Parameters.Add("@TranslatedColumnNames", SqlDbType.NVarChar).Value = query.TranslatedColumnNames;
            updateQuery.Parameters.Add("@UpdatePeriodTicks", SqlDbType.BigInt).Value = query.UpdatePeriodTicks;
            updateQuery.Parameters.Add("@Name", SqlDbType.NVarChar).Value = query.Name;
            updateQuery.ExecuteNonQuery();
        }
    }
}
