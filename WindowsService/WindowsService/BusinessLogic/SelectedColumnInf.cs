﻿namespace WindowsService
{
    public class SelectedColumnInf
    {
        public string AliasTableName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string AliasColumnName { get; set; }
        public string DataType { get; set; }
        public int CharacterMaxLength { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
    }
}