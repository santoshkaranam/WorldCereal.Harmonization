using IIASA.Dataset.Gdal.Geo;
using OSGeo.OGR;

namespace IIASA.Dataset.Gdal.Feature
{
    public class PolygonFeature: BaseFeature
    {
        [FieldMetaData(FieldType = FieldType.OFTReal, Length = 64, Name = "area")]
        public double Area { get; set; }

        // will be added by default
        public XY[] Geometry;
    }

    public class FeatureMetaData
    {
        public double Area { get; set; }

        public int Count { get; set; }

        public string ColumnValue { get; set; }

        public string ExtendedData { get; set; } = "";
    }
}