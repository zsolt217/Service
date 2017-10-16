using System;
using System.Collections.Generic;

namespace WindowsService
{

    class PrepareSQL
    {
        Dictionary<string, string> _nameAndAlias = new Dictionary<string, string>();
        string[] _SQLLines;
        public PrepareSQL(string sQL)
        {
            _SQLLines = sQL.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            GetNameAndAlias();
            ;

        }

        private void GetNameAndAlias()
        {
            foreach (var line in _SQLLines)
            {
                if (!line.Contains("dbo.["))
                    continue;

                string key = string.Empty;
                int i = line.IndexOf("dbo.[") + 5;
                while (i < line.Length && line[i] != ']')
                {
                    key += line[i];
                    i++;
                }
                i += 2;//szóköz és a ] átlépése
                if (i >= line.Length)
                    continue;

                string value = string.Empty;

                while (i < line.Length && line[i] != ' ')
                {
                    value += line[i];
                    i++;
                }
                _nameAndAlias.Add(key, value);
            }

        }
    }
}
