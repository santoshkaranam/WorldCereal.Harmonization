using System;
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
                var classCodeIndex = GetIndex(reader, CsvConstants.ClassLegendValue);
                var lcIndex = GetIndex(reader, CsvConstants.LC);
                var ct1Index = GetIndex(reader, CsvConstants.CT1);
                var ct2Index = GetIndex(reader, CsvConstants.CT2);
                var irr1Index = GetIndex(reader, CsvConstants.Irr1);
                var irr2Index = GetIndex(reader, CsvConstants.Irr2);
                var irr3Index = GetIndex(reader, CsvConstants.Irr3);
                while (reader.Read())
                {
                    string code = reader.GetField<string>(classCodeIndex).Trim();
                    int lc = reader.GetField<int>(lcIndex);
                    int ct1 = reader.GetField<int>(ct1Index);
                    int ct2 = reader.GetField<int>(ct2Index);
                    int irr1 = reader.GetField<int>(irr1Index);
                    int irr2 = reader.GetField<int>(irr2Index);
                    int irr3 = reader.GetField<int>(irr3Index);
                    records.Add(new MapData
                    {
                        ColumnValue = code, LandCover = lc, CropType1 = ct1, CropType2 = ct2, Irrigation1 = irr1,
                        Irrigation2 = irr2, Irrigation3 = irr3
                    });
                }
            }

            return records.ToArray();
        }

        private int GetIndex(CsvReader reader, string colHeaderValue)
        {
            for (int i = 0; i < 10; i++)
            {
                var value = reader.GetField<string>(i);
                if (value.ToLowerInvariant() == colHeaderValue.ToLowerInvariant())
                {
                    return i;
                }
            }

            throw new Exception($"{colHeaderValue} column header not found in the csv");
        }

        private void BadDataFunc(ReadingContext readingContext)
        {
            _logger.Line($"Error- Bad data found {readingContext.Field}");
        }
    }

    public static class CsvConstants    
    {
        public const string LC = "LC";
        public const string CT1 = "CT1";
        public const string CT2 = "CT2";
        public const string Irr1 = "Irr1";
        public const string Irr2 = "Irr2";
        public const string Irr3 = "Irr3";
        public const string AllocationCount = "AllocationCount";
        public const string ClassLegendValue = "ClassLegendValue";  
    }
}