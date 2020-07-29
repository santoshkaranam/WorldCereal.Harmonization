using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using IIASA.Dataset.Gdal.Feature;
using IIASA.Dataset.Harmonization.Interface;

namespace IIASA.Dataset.Harmonization.Core
{
    class HarmonizedDatasetGenerator : IHarmonizedPackageGenerator
    {
        private readonly ILogger _logger;
        private readonly MapDataReader _mapDataReader;
        private readonly ShpFileReader _shpFileReader;
        private readonly PolygonVectorFileGenerator _vectorFileGenerator;
        private readonly Config _config;

        public HarmonizedDatasetGenerator(ILogger logger, MapDataReader mapDataReader, ShpFileReader shpFileReader, PolygonVectorFileGenerator vectorFileGenerator, Config config)
        {
            _mapDataReader = mapDataReader;
            _shpFileReader = shpFileReader;
            _vectorFileGenerator = vectorFileGenerator;
            _config = config;
            _logger = logger;
        }

        public void Generate(string shpPath, string csvPath, DateTime dateTime, string fileName)
        {
            _logger.Line("Reading map data...");
            MapData[] mapRecords = _mapDataReader.Read(csvPath);
            _logger.Line("Reading map data completed!");

            var featureList = new BlockingCollection<BaseFeature>(1000);

            _logger.Line("Reading shp file and extracting features...");
            var readTask = Task.Factory.StartNew(() =>
                _shpFileReader.Read(shpPath, mapRecords, dateTime, featureList));

            _logger.Line("Creating vector file from features...");
            _vectorFileGenerator.CreateVectorFile(featureList, $"{fileName}.{_config.OutPutDriver.ToLowerInvariant()}",
                _config.OutPutDriver, "cropData");
            _logger.Line("Creating vector file from features completed!");

            Task.WaitAll(readTask);
        }
    }
}