using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IIASA.Dataset.Gdal;
using IIASA.Dataset.Gdal.Feature;
using IIASA.Dataset.Gdal.Layer;
using IIASA.Dataset.Harmonization.Interface;

namespace IIASA.Dataset.Harmonization.Core
{
    public abstract class VectorFileGenerator
    {
        private readonly ILogger _logger;

        public VectorFileGenerator(ILogger logger)
        {
            _logger = logger;
        }
        public void CreateVectorFile(BlockingCollection<BaseFeature> features, string path, string driverName, string layerName)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (GdalWrapper gdalWrapper = new GdalWrapper())
            {
                gdalWrapper.InitializeDatasetForCreate(path, driverName);
                using (LayerWrapper layer = CreateLayer(layerName, gdalWrapper))
                {
                    IDictionary<string, string> propNameToFieldName = CreateFieldsAndGetNameMap(layer);
                    try
                    {
                        int index = 0;
                        while (true)
                        {
                            var data = features.Take();
                            //_logger.Line($"Adding feature to {driverName} with sampleId- {data.SampleId}");
                            CreateFeatureWrapper(layer, propNameToFieldName, data);
                            index++;

                            if (index == 1000)
                            {
                                _logger.Line($"Writing Data 1000 features to disk.");
                                layer.WriteToDisk();
                                gdalWrapper.Flush();
                                index = 0;
                            }
                        }

                    }
                    catch (InvalidOperationException e)
                    {
                        _logger.Line("All features added!");
                    }
                }
            }
        }

        private void CreateFeatureWrapper(LayerWrapper layer, IDictionary<string, string> propNameToFieldName, BaseFeature data)
        {
            FeatureWrapper feature = layer.CreateFeature();

            feature.Set(propNameToFieldName[nameof(data.SampleId)], data.SampleId);
            feature.Set(propNameToFieldName[nameof(data.ValidityTime)], data.ValidityTime.Value);
            feature.Set(propNameToFieldName[nameof(data.ExtendedData)], data.ExtendedData);
            feature.Set(propNameToFieldName[nameof(data.UserConf)], data.UserConf);
            feature.Set(propNameToFieldName[nameof(data.LandCover)], data.LandCover);
            feature.Set(propNameToFieldName[nameof(data.CropType1)], data.CropType1);
            feature.Set(propNameToFieldName[nameof(data.CropType2)], data.CropType2);
            feature.Set(propNameToFieldName[nameof(data.Irrigation1)], data.Irrigation1);
            feature.Set(propNameToFieldName[nameof(data.Irrigation2)], data.Irrigation2);
            feature.Set(propNameToFieldName[nameof(data.Irrigation3)], data.Irrigation3);

            SetTypeSpecificData(feature, propNameToFieldName, data);

            layer.AddFeatureToLayer(feature);
        }

        internal abstract LayerWrapper CreateLayer(string layerName, GdalWrapper gdalWrapper);
       

        internal abstract void SetTypeSpecificData(FeatureWrapper feature, IDictionary<string, string> propNameToFieldName,
            BaseFeature baseFeature);
        

        private IDictionary<string, string> CreateFieldsAndGetNameMap(LayerWrapper layer)
        {
            PropertyInfo[] propertyInfos = typeof(PolygonFeature).GetProperties();
            Dictionary<string, string> propNameToFieldName = new Dictionary<string, string>();
            foreach (PropertyInfo prop in propertyInfos)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                FieldMetaDataAttribute fieldAttr =
                    attrs.FirstOrDefault(attr => (attr as FieldMetaDataAttribute) != null) as FieldMetaDataAttribute;
                if (fieldAttr == null)
                {
                    continue;
                }

                layer.CreateField(fieldAttr.Name, fieldAttr.FieldType, fieldAttr.Length);

                propNameToFieldName.Add(prop.Name, fieldAttr.Name);
            }

            return propNameToFieldName;
        }
    }
}