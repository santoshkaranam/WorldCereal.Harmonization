using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using IIASA.Dataset.Harmonization.Core;
using IIASA.Dataset.Harmonization.Interface;
using Newtonsoft.Json;

namespace IIASA.Dataset.Harmonization
{
    class Program
    {
        private static Logger _logger;
        private static Config _config;

        static void Main(string[] args)
        {
            _logger = new Logger();
            while (true)
            {
                Run();
            }
        }

        private static void Run()
        {
            LoadConfig();
            _logger.Line("-----------Dataset harmonization---------------");
            _logger.Line("1. Read and output legend output from shp file");
            _logger.Line("2. Generate Harmonized Geopackage");
            _logger.Line("3. Reload config from appsettings.json");
            _logger.Line("Enter option or Press any key to exit");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int option) == false)
            {
                Environment.Exit(-1);
            }

            switch (option)
            {
                case 1:
                    ReadMetaDataAndOutputCsv();
                    break;

                case 2:
                    GenerateHarmonizedGeoPackage();
                    break;

                case 3:
                    _logger.Line("Reloading config from appsettings.json...");
                    break;

                default:
                    _logger.Line("Enter one among the provided options");
                    break;
            }
        }

        private static void ReadMetaDataAndOutputCsv()
        {
            if (AskShpFilePath(out var shpPath)) return;

            var reader = new ShpFileReader(_logger, _config);

            var datas = reader.ReadMetadata(shpPath);
            var builder = new StringBuilder();
            builder.AppendLine($"Dataset:,{shpPath}");
            double total = datas.Sum(x => x.Count);
            builder.AppendLine($"TotalCount:{total}");
            var area = datas.Sum(x => x.Area);
            builder.AppendLine($"TotalArea:{area}");
            builder.AppendLine("Class,TotalArea,Area%,TotalCount,Count%,LC,CT1,CT2,Irr1,Irr2,Irr3");
            var orderedList =  datas.OrderBy(x => x.ColumnValue);
            foreach (var data in orderedList)
            {
                var line = data.ColumnValue;
                line += $",{data.Area}";
                line += $",{data.Area/area:##.000000}";
                line += $",{data.Count}";
                line += $",{data.Count/total:##.0000}";
                builder.AppendLine(line);
            }

            File.WriteAllBytes($".\\ClassDetails.csv", Encoding.UTF8.GetBytes(builder.ToString()));
        }

        private static void LoadConfig()
        {
            _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(".\\appsettings.json"));
        }

        private static void GenerateHarmonizedGeoPackage()
        {
            _logger.Line("Enter path to Legend mapping csv.");
            string path = Console.ReadLine();

            if (File.Exists(path) == false)
            {
                _logger.Line("Invalid Path!");
                return;
            }

            if (AskShpFilePath(out var shpPath)) return;

            IHarmonizedPackageGenerator generator = new HarmonizedDatasetGenerator(_logger, new MapDataReader(_logger),
                new ShpFileReader(_logger,_config), new PolygonVectorFileGenerator(_logger), _config);

            DateTime dateTime = DateTime.ParseExact(_config.LpisDate, "yyyy/M/d",CultureInfo.InvariantCulture);
            generator.Generate(shpPath,path,dateTime, $"{dateTime.Year}_{_config.CountryCode}_POLY1_110");
        }

        private static bool AskShpFilePath(out string shpPath)
        {
            _logger.Line("Enter path to shp file folder");
            shpPath = Console.ReadLine();

            if (Directory.Exists(shpPath) == false)
            {
                _logger.Line("Invalid Path!");
                return true;
            }

            return false;
        }
    }

    public class Config
    {
        public int MaxFeatureToReadFromShp { get; set; }
        public string InputDriverName { get; set; }
        public string ColNameToRead { get; set; }
        public string OutPutDriver { get; set; }
        public string LpisDate { get; set; }
        public string CountryCode { get; set; }
    }
}
