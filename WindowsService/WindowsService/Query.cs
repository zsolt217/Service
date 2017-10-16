using System;

namespace WindowsService
{
    class Query
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string GUID { get; set; }
        public string SQL { get; set; }
        public string ResultTableName { get; set; }
        public string TranslatedColumnNames { get; set; }
        public DateTime NextUpdating { get; set; }
        public TimeSpan UpdatePeriod { get; set; }
        public long UpdatePeriodTicks
        {
            get
            {
                return UpdatePeriod.Ticks;
            }
            set
            {
                UpdatePeriod = TimeSpan.FromTicks(value);
            }
        }
    }
}
