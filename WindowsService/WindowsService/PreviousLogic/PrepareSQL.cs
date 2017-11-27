//using System;
//using System.Collections.Generic;

//namespace WindowsService
//{
//    public class PrepareSQL
//    {
//        private Dictionary<string, string> _aliasAndName;
//        private string[] _SQLLines;
//        private List<SelectedColumnInf> preparedSQLInf;

//        public PrepareSQL(string sQL)
//        {
//            _SQLLines = sQL.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
//            preparedSQLInf = new List<SelectedColumnInf>();
//            _aliasAndName = new Dictionary<string, string>();
//        }

//        public List<SelectedColumnInf> GetPrepared()
//        {
//            GetNameAndAlias();
//            GetSelectedColumn();
//            return preparedSQLInf;
//        }

//        private void GetSelectedColumn()
//        {
//            foreach (var line in _SQLLines)
//            {
//                if (line.Contains("FROM"))//vége a selecteknek, nem folytatjuk
//                    return;

//                string trimmedLine = line.Trim();
//                //elvárt pl sor:TBL188.[Name] ''CompanyProfile.Name'',

//                int dot = trimmedLine.IndexOf('.');

//                if (dot < 0)//nincs a sorban pont
//                    continue;

//                string tableAlias = trimmedLine.Substring(0, dot);
//                if (_aliasAndName.ContainsKey(tableAlias))
//                {
//                    var struc = new SelectedColumnInf();
//                    struc.AliasTableName = tableAlias;
//                    string TableName;
//                    _aliasAndName.TryGetValue(tableAlias, out TableName);
//                    struc.TableName = TableName;
//                    dot += 2; //köv karakter [
//                    while (dot < trimmedLine.Length && trimmedLine[dot] != ']')
//                    {
//                        struc.ColumnName += trimmedLine[dot];
//                        dot++;
//                    }

//                    dot += 4; //szóköz+2 db ''
//                    while (dot < trimmedLine.Length && trimmedLine[dot] != '\'')
//                    {
//                        struc.AliasColumnName += trimmedLine[dot];
//                        dot++;
//                    }

//                    preparedSQLInf.Add(struc);
//                }
//            }
//        }

//        private void GetNameAndAlias()
//        {
//            foreach (var line in _SQLLines)
//            {
//                if (!line.Contains("dbo.["))
//                    continue;

//                string value = string.Empty;
//                int i = line.IndexOf("dbo.[") + 5;
//                while (i < line.Length && line[i] != ']')
//                {
//                    value += line[i];
//                    i++;
//                }
//                i += 2;//szóköz és a ] átlépése
//                if (i >= line.Length)
//                    continue;

//                string key = string.Empty;

//                while (i < line.Length && line[i] != ' ')
//                {
//                    key += line[i];
//                    i++;
//                }
//                _aliasAndName.Add(key, value);
//            }
//        }
//    }
//}