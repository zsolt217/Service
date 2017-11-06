using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace WindowsService
{
    internal class PreviousLogic
    {
        private SqlCommand _commandGetQueries = new SqlCommand(@"SELECT Id                                          , NextUpdating
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

        public PreviousLogic()
        {
            _queries = new List<Query>();
            ReadQueries();
            foreach (var query in _queries)
            {
                DataTable dt = new DataTable();
                dt.Load((new SqlCommand(query.SQL, ConnectionFactory.GetSourceConnection)).ExecuteReader()); //query lefuttatása

                if (String.IsNullOrEmpty(query.ResultTableName))
                {
                    query.ResultTableName = CreateTable(new PrepareSQL(query.SQL).GetPrepared(), dt.Columns);
                    UpdateQuery(query);
                }
                CopyData(dt, query.ResultTableName);
            }
        }

        private void CopyData(DataTable dt, string resultTableName)
        {
            DataTable DestColumnAndType = GetDestColumnAndType(resultTableName);
            string columns = String.Empty;
            SqlCommand cmd = new SqlCommand();

            foreach (DataRow row in DestColumnAndType.Rows)
            {
                if (String.IsNullOrEmpty(columns))
                    columns += "@" + row["COLUMN_NAME"].ToString() + " ";
                else
                    columns += ",@" + row["COLUMN_NAME"].ToString() + " ";
                cmd.Parameters.Add("@" + row["COLUMN_NAME"].ToString(), GetDestColumnType(row["DATA_TYPE"].ToString()));
            }
            cmd.CommandText = String.Format("insert into {0} ({1}) VALUES ({2})", resultTableName, columns.Replace("@", String.Empty), columns);
            cmd.Connection = ConnectionFactory.GetDestinationConnection;

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    cmd.Parameters[i].Value = GetValueInSuitableFormat(row[dt.Columns[i].ToString()].ToString(), cmd.Parameters[i].SqlDbType);
                }
                cmd.ExecuteNonQuery();
            }
        }

        private object GetValueInSuitableFormat(string data, SqlDbType sqlDbType)
        {
            if (String.IsNullOrEmpty(data))
                return DBNull.Value;

            switch (sqlDbType)
            {
                case SqlDbType.Bit:
                    return bool.Parse(data);

                case SqlDbType.DateTime:
                    return DateTime.Parse(data);

                case SqlDbType.Decimal:
                    return decimal.Parse(data);

                case SqlDbType.Int:
                    return int.Parse(data);

                case SqlDbType.NVarChar:
                    return data;

                default:
                    return data;
            }
        }

        private SqlDbType GetDestColumnType(string dataType)
        {
            switch (dataType)//ha ezt bővítem akk ezt is kell: GetValueInSuitableFormat
            {
                case "bit":
                    return SqlDbType.Bit;

                case "datetime":
                    return SqlDbType.DateTime;

                case "decimal":
                    return SqlDbType.Decimal;

                case "int":
                    return SqlDbType.Int;

                case "nvarchar":
                    return SqlDbType.NVarChar;

                default:
                    return SqlDbType.NVarChar;
            }
        }

        /// <summary>
        /// Visszaadott DataTable oszlopai: COLUMN_NAME, DATA_TYPE
        /// </summary>
        /// <param name="resultTableName"></param>
        /// <returns></returns>
        private DataTable GetDestColumnAndType(string resultTableName)
        {
            //hiba volt: ehhez a command parancshoz már tartozik megnyitott datareader, először ezt zárjuk be => connection stringhez hozzáadni MultipleActiveResultSets=True;
            DataTable dt = new DataTable();
            dt.Load((new SqlCommand(String.Format("select COLUMN_NAME, DATA_TYPE from information_schema.columns where TABLE_NAME = '{0}'", resultTableName), ConnectionFactory.GetDestinationConnection)).ExecuteReader());
            return dt;
        }

        private void UpdateQuery(Query query)
        {
            new SqlCommand(String.Format("update Query set ResultTableName = '{0}' where Id = {1}", query.ResultTableName, query.Id), ConnectionFactory.GetDestinationConnection).ExecuteNonQuery();
        }

        private string _columnsWithType;

        private string CreateTable(List<SelectedColumnInf> list, DataColumnCollection dtColl)
        {
            _columnsWithType = String.Empty;
            GetDataTypeOfListItems(list);
            foreach (var column in dtColl)
            {
                var casualSelect = list.Find(x => x.AliasColumnName == column.ToString());
                if (casualSelect != null && !String.IsNullOrEmpty((casualSelect as SelectedColumnInf)?.DataType))
                { BuildColumnsWithType(column.ToString(), (casualSelect as SelectedColumnInf).DataType, (casualSelect as SelectedColumnInf).CharacterMaxLength, (casualSelect as SelectedColumnInf).NumericPrecision, (casualSelect as SelectedColumnInf).NumericScale); }
                else//ha nem tudjuk a típusát, vagy a lekérdezés miatt nagyon bonyolult akk nvarchar lesz
                { BuildColumnsWithType(column.ToString(), "nvarchar", -1, 0, 0); }
            }
            string tableName = "a" + Guid.NewGuid().ToString().Replace("-", String.Empty); //azért fűzök hozzá egy karaktert, mert a GUID kezdődhet számmal is, az pedig nem valid táblanév

            string createTable = String.Format("create table {0} ({1})", tableName, _columnsWithType);

            new SqlCommand(createTable, ConnectionFactory.GetDestinationConnection).ExecuteNonQuery();//TODO: felkészíteni a hibakezelésre h ha nem fut le jól a beírás
            return tableName;
        }

        private void BuildColumnsWithType(string columName, string columnType, int CharacterMaxLength, int NumericPrecision, int NumericScale)
        {
            //több probléma is volt az oszlopelnevezésekkel pl: X.Y, X (HUF) => X_Y, X_HUF_
            columName = columName.Replace(".", "_").Replace(" ", String.Empty).Replace("(", "_").Replace(")", "_").Replace("%", String.Empty);
            columName = RemoveDiacritics(columName);
            string lengthConstraints = String.Empty;
            if (columnType.Equals("nvarchar"))
                lengthConstraints = String.Format("({0})", CharacterMaxLength == -1 ? "max" : CharacterMaxLength.ToString());
            else if (columnType.Equals("decimal"))
                lengthConstraints = String.Format("({0},{1})", NumericPrecision, NumericScale);

            if (String.IsNullOrEmpty(_columnsWithType))
                _columnsWithType += String.Format("[{0}] [{1}] {2}", columName, columnType, lengthConstraints);
            else
                _columnsWithType += String.Format(",[{0}] [{1}] {2}", columName, columnType, lengthConstraints);
        }

        /// <summary>
        /// Az ékezeteket távolítja el azért hogy valid oszlopneveket kapjunk
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void GetDataTypeOfListItems(List<SelectedColumnInf> list)
        {
            foreach (var select in list)
            {
                string DataType;
                int CharacterMaxLength;
                int NumericPrecision;
                int NumericScale;
                GetType(select.TableName, select.ColumnName, out DataType, out CharacterMaxLength, out NumericPrecision, out NumericScale);//ha nulla akk stringként fogjuk kezelni
                select.DataType = DataType;
                select.CharacterMaxLength = CharacterMaxLength;
                select.NumericPrecision = NumericPrecision;
                select.NumericScale = NumericScale;
            }
        }

        private void GetType(string tableName, string columnName, out string dataType, out int CharacterMaxLength, out int NumericPrecision, out int NumericScale)
        {
            string query = String.Format("select DATA_TYPE , CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE from information_schema.columns where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'", tableName, columnName);
            DataTable dt = new DataTable();
            dt.Load((new SqlCommand(query, ConnectionFactory.GetSourceConnection)).ExecuteReader());
            if (dt.Rows.Count > 0)
            {
                dataType = dt.Rows[0]["DATA_TYPE"].ToString();
                int.TryParse(dt.Rows[0]["CHARACTER_MAXIMUM_LENGTH"].ToString(), out CharacterMaxLength);
                int.TryParse(dt.Rows[0]["NUMERIC_PRECISION"].ToString(), out NumericPrecision);
                int.TryParse(dt.Rows[0]["NUMERIC_SCALE"].ToString(), out NumericScale);
            }
            else
            {
                dataType = string.Empty;
                CharacterMaxLength = 0;
                NumericScale = 0;
                NumericPrecision = 0;
            }
        }

        private void ReadQueries()
        {
            //_sqlcommand.ExecuteNonQuery();
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
                        UpdatePeriodTicks = long.Parse(dr["UpdatePeriodTicks"].ToString())
                    };
                    _queries.Add(query);
                }
            }
        }
    }
}