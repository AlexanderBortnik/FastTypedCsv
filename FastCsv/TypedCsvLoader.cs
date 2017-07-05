using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LumenWorks.Framework.IO.Csv;

namespace FastCsv
{
    public static class TypedCsvLoader
    {
        public static IEnumerable<T> LoadCsv<T>(string filePath) where T : new()
        {
            var loader = TypedObjectLoader.Create<T>();
            using (CsvReader csv = new CsvReader(new StreamReader(filePath), true))
            {
                var headers = csv.GetFieldHeaders();
                var objectProperties = headers.ToDictionary(k => k, v => default(string));
                while (csv.ReadNextRecord())
                {
                    foreach (var key in objectProperties.Keys)
                    {
                        objectProperties[key] = csv[key];
                    }
                    yield return loader(objectProperties);
                }
            }
        }
    }
}