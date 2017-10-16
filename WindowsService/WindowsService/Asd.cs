using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WindowsService
{
    class Asd
    {
        SqlCommand _sqlcommand = new SqlCommand(@"SELECT Id
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
        List<Query> _queries;

        public Asd()
        {
            ReadQueries();

            foreach (var query in _queries)
            {
                new PrepareSQL(query.SQL);

                DataTable dt = new DataTable();
                dt.Load((new SqlCommand(query.SQL, ConnectionFactory.GetSourceConnection)).ExecuteReader());
                
            }
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
