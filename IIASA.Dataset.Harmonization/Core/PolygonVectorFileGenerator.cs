using System.Collections.Generic;
using IIASA.Dataset.Gdal;
using IIASA.Dataset.Gdal.Feature;
using IIASA.Dataset.Gdal.Layer;
using IIASA.Dataset.Harmonization.Interface;

namespace IIASA.Dataset.Harmonization.Core
{
    public class PolygonVectorFileGenerator : VectorFileGenerator
    {
        public PolygonVectorFileGenerator(ILogger logger) : base(logger)
        {
        }

        internal override void SetTypeSpecificData(FeatureWrapper feature,
            IDictionary<string, string> propNameToFieldName, BaseFeature baseFeature)
        {
            var data = baseFeature as PolygonFeature;
            feature.Set(propNameToFieldName[nameof(data.Area)], data.Area);
            feature.SetPolygonGeo(data.Geometry);
        }

        internal override LayerWrapper CreateLayer(string layerName, GdalWrapper gdalWrapper)
        {
            return gdalWrapper.CreateWGS84PolygonLayer(layerName);
        }
    }
}