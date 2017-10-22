using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WindowsService
{
    internal class MainLogic
    {
        private SqlCommand _sqlcommand = new SqlCommand(@"SELECT Id
                                              , NextUpdating
                                              , ResultTableName
                                              , SQL
                                              , TranslatedColumnNames
                                              , Name
                                              , CreationDate
                                              , GUID
                                              , ModifyDate
                                          ,  UpdatePeriodTicks
                                          FROM Query",
            ConnectionFactory.GetDestinationConnection);

        private List<Query> _queries;

        public MainLogic()
        {
            ReadQueries();

            foreach (var query in _queries)
            {
                DataTable dt = new DataTable();
                dt.Load((new SqlCommand(query.SQL, ConnectionFactory.GetSourceConnection)).ExecuteReader()); //query lefuttatása

                if (String.IsNullOrEmpty(query.ResultTableName))
                {
                    query.ResultTableName = CreateTable(new PrepareSQL(query.SQL).GetPrepared(), dt.Columns);
                }
            }
        }

        private string CreateTable(List<SelectedColumnInf> list, DataColumnCollection dtColl)
        {
            foreach (var column in dtColl)
            {
            }

            string tableName = Guid.NewGuid().ToString().Replace("-", String.Empty);
            return tableName;
        }

        private void ReadQueries()
        {
            _sqlcommand.ExecuteNonQuery();
            SqlDataReader dr = _sqlcommand.ExecuteReader();
            _queries = new List<Query>();
            while (dr.Read())
            {
                Query query = new Query();
                query.Id = int.Parse(dr["Id"].ToString());
                query.NextUpdating = DateTime.Parse(dr["NextUpdating"].ToString());
                query.ResultTableName = dr["ResultTableName"].ToString();
                query.SQL = dr["SQL"].ToString();
                query.UpdatePeriodTicks = long.Parse(dr["UpdatePeriodTicks"].ToString());
                _queries.Add(query);
            }
        }
    }
}