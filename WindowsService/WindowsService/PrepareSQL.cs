using System;
using System.Collections.Generic;

namespace WindowsService
{

    class PrepareSQL
    {
        Dictionary<string, string> _nameAndAlias;
        string[] _SQLLines;
        public PrepareSQL(string sQL)
        {

            _SQLLines = sQL.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            GetNameAndAlias();

        }

        private void GetNameAndAlias()
        {
            foreach (var line in _SQLLines)
            {
                if (!line.Contains("dbo.["))
                    continue;

                string key = string.Empty;
                int i = line.IndexOf("dbo.[") + 5;
                while (line[i] != ']' && i <= line.Length)
                {
                    key += line[i];
                    i++;
                }
                i += 2;//szóköz és a ] átlépése
                if (i >= line.Length)
                    continue;

                string value = string.Empty;

                //if
            }

        }
    }
}
