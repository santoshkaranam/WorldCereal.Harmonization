using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using IIASA.Dataset.Harmonization.Interface;

namespace IIASA.Dataset.Harmonization.Core
{
    public class MapDataReader
    {
        private readonly ILogger _logger;

        public MapDataReader(ILogger logger)
        {
            _logger = logger;
        }
        public MapData[] Read(string csvPath)
        {
            List<MapData> records = new List<MapData>();
            using (CsvReader reader = new CsvReader(new StreamReader(csvPath, Encoding.UTF8),
                new CsvConfiguration(CultureInfo.InvariantCulture) {Delimiter = ",", BadDataFound = BadDataFunc}))
            {
                reader.Read();
                while (reader.Read())
                {
                    string code = reader.GetField<string>(0).Trim();
                    int lc = reader.GetField<int>(1);
                    int ct1 = reader.GetField<int>(2);
                    int ct2 = reader.GetField<int>(3);
                    records.Add(new MapData { ColumnValue = code, LandCover = lc, CropType1 = ct1, CropType2 = ct2 });
                }
            }

            return records.ToArray();
        }

        private void BadDataFunc(ReadingContext readingContext)
        {
            _logger.Line($"Error- Bad data found {readingContext.Field}");
        }
    }
}