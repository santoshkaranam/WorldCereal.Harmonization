using System.Collections.Generic;
using IIASA.Dataset.Gdal.Geo;

namespace IIASA.Dataset.Gdal.Feature
{
    public class PolygonFeatureGeoData
    {
        public string ColumnValue { get; set; }
        public double Area { get; set; }
        public IList<XY> Coordinates { get; set; }
    }
}