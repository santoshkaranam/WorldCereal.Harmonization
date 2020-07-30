using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IIASA.Dataset.Gdal;
using IIASA.Dataset.Gdal.Feature;
using IIASA.Dataset.Gdal.Layer;
using IIASA.Dataset.Harmonization.Interface;

namespace IIASA.Dataset.Harmonization.Core
{
    public class ShpFileReader
    {
        private readonly ILogger _logger;
        private readonly Config _config;
        private string _missingMapsFileName = ".\\missingCodes.txt";

        public ShpFileReader(ILogger logger, Config config)
        {
            _logger = logger;
            _config = config;

            if (File.Exists(_missingMapsFileName))
            {
                File.Delete(_missingMapsFileName);
            }
        }

        public IList<FeatureMetaData> ReadMetadata(string shpFilePath)
        {
            var concurrentDictionary = new ConcurrentDictionary<string,FeatureMetaData>();
            var featureCount = GetFeatureCount(shpFilePath);

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * _config.ConcurrencyMultiplier;
            var maxCountToRead = featureCount / concurrencyLevel;
            var taskList = new List<Task>();
            for (int taskIndex = 0; taskIndex < concurrencyLevel; taskIndex++)
            {
                long startIndex = taskIndex * maxCountToRead;
                long endIndex = (taskIndex + 1) * maxCountToRead;
                if (endIndex > featureCount)
                {
                    endIndex = featureCount;
                }

                _logger.Line($"MainTread-{Environment.CurrentManagedThreadId} - S-{startIndex} E-{endIndex} Total-{endIndex - startIndex}");

                var task = Task.Factory.StartNew(()=>CreateMetaDataReadTask(shpFilePath, endIndex, concurrentDictionary, startIndex));
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            return concurrentDictionary.Values.ToList();
        }

        private void CreateMetaDataReadTask(string shpFilePath, long endIndex,
            ConcurrentDictionary<string, FeatureMetaData> concurrentDictionary,
            long startIndex)
        {

            _logger.Line(
                $"{Environment.CurrentManagedThreadId} - S-{startIndex} E-{endIndex} Total-{endIndex - startIndex}");

            using var gdalWrapper = new GdalWrapper();
            gdalWrapper.Configure();
            gdalWrapper.InitializeDatasetForRead(shpFilePath, _config.InputDriverName);
            IEnumerable<LayerWrapper> layers = gdalWrapper.GetLayers();
            LayerWrapper layerWrapper = layers.First();

            for (; startIndex < endIndex; startIndex++)
            {
                var featureWrapper = layerWrapper.GetFeature(startIndex);
                if (featureWrapper == null) continue;
                var area = featureWrapper.GetArea();
                var columnValue = featureWrapper.GetFieldAsString(_config.ColNameToRead);
                var extData = "";
                foreach (var colName in _config.ColsToReadAsExtendedData)
                {
                    extData += featureWrapper.GetFieldAsString(colName) + ";";
                }

                concurrentDictionary.AddOrUpdate(columnValue,
                    new FeatureMetaData {Area = area, Count = 1, ColumnValue = columnValue, ExtendedData = extData},
                    (key, value) =>
                    {
                        value.Count++;
                        value.Area += area;
                        return value;
                    });

                if ((endIndex - startIndex) % 1000 == 0)
                {
                    var sum = concurrentDictionary.Values.Sum(x => x.Count);

                    _logger.Line(
                        $"{Environment.CurrentManagedThreadId} S-{startIndex} E-{endIndex} Remaining-{endIndex - startIndex}, TotalRead-{sum}");
                    if (sum > _config.MaxFeatureToReadFromShp)
                    {
                        break;
                    }
                }
            }
        }

        private long GetFeatureCount(string shpFilePath)
        {
            long featureCount = 0;
            using (var gdalWrapper = new GdalWrapper())
            {
                gdalWrapper.Configure();
                gdalWrapper.InitializeDatasetForRead(shpFilePath, _config.InputDriverName);
                IEnumerable<LayerWrapper> layers = gdalWrapper.GetLayers();
                LayerWrapper layerWrapper = layers.First();
                var layerData = layerWrapper.GetData();
                _logger.Line($"Layer Feature count - {featureCount}");
                _logger.Line($"Layer Projection - {layerData.ProjectionName}");
                featureCount = layerData.FeatureCount;
                layerWrapper.Dispose();
            }

            return featureCount;
        }

        public void Read(string shpPath, MapData[] map, DateTime dateTime, BlockingCollection<BaseFeature> featureList)
        {
            var featureCount = GetFeatureCount(shpPath);

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * _config.ConcurrencyMultiplier;
            var maxCountToRead = featureCount / concurrencyLevel;
            var taskList = new List<Task>();
            for (int taskIndex = 0; taskIndex < concurrencyLevel; taskIndex++)
            {
                long startIndex = taskIndex * maxCountToRead;
                long endIndex = (taskIndex + 1) * maxCountToRead;
                if (endIndex > featureCount)
                {
                    endIndex = featureCount;
                }

                _logger.Line(
                    $"MainTread-{Environment.CurrentManagedThreadId} - S-{startIndex} E-{endIndex} Total-{endIndex - startIndex}");

                var mapDataList = map.ToArray();
                var task = CreateFeatureReadTask(shpPath, dateTime, featureList, endIndex, mapDataList, startIndex);
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());
            featureList.CompleteAdding();
        }

        private Task CreateFeatureReadTask(string shpPath, DateTime dateTime,
            BlockingCollection<BaseFeature> featureList, long endIndex,
            MapData[] mapDataList, long startIndex)
        {
            var task = Task.Factory.StartNew(() =>
                {
                    _logger.Line(
                        $"{Environment.CurrentManagedThreadId} -Read Start S-{startIndex} E-{endIndex} Total-{endIndex - startIndex}");

                    using (var gdalWrapper = new GdalWrapper())
                    {
                        gdalWrapper.Configure();
                        gdalWrapper.InitializeDatasetForRead(shpPath, _config.InputDriverName);
                        IEnumerable<LayerWrapper> layers = gdalWrapper.GetLayers();
                        LayerWrapper layerWrapper = layers.First();

                        for (; startIndex < endIndex; startIndex++)
                        {
                            var featureWrapper = layerWrapper.GetFeature(startIndex);
                            if (featureWrapper == null) continue;
                            PolygonFeatureGeoData featureData = featureWrapper.GetFieldGeo();
                            featureData.ColumnValue = featureWrapper.GetFieldAsString(_config.ColNameToRead);
                            featureList.Add(GetPolygonFeatureData(featureData, mapDataList, dateTime));

                            if ((endIndex - startIndex) % 1000 == 0)
                            {
                                _logger.Line(
                                    $"{Environment.CurrentManagedThreadId} S-{startIndex} E-{endIndex} Remaining-{endIndex - startIndex}");
                            }
                        }
                    }
                }
            );

            return task;
        }

        private PolygonFeature GetPolygonFeatureData(PolygonFeatureGeoData featureGeoData, IEnumerable<MapData> records,
            DateTime date)
        {
            var code = featureGeoData.ColumnValue.Trim();
            MapData data = records.FirstOrDefault(x => x.ColumnValue == code);
            PolygonFeature featureData = new PolygonFeature
            {
                Area = featureGeoData.Area,
                SampleId = Guid.NewGuid().ToString("N"),
                Geometry = featureGeoData.Coordinates.ToArray(),
                ValidityTime = date,
                ExtendedData = featureGeoData.ColumnValue,
                UserConf = 100
            };

            if (data == null)
            {
                _logger.Line($"Failed to map feature data - {code}");
                File.AppendAllLines(_missingMapsFileName, new[] { featureGeoData.ColumnValue});
            }
            else
            {
                featureData.LandCover = data.LandCover;
                featureData.CropType1 = data.CropType1;
                featureData.CropType2 = data.CropType2;
                featureData.Irrigation1 = data.Irrigation1;
                featureData.Irrigation2 = data.Irrigation2;
                featureData.Irrigation3 = data.Irrigation3;
            }

            return featureData;
        }
    }
}