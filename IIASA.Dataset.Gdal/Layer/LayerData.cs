using IIASA.Dataset.Gdal.Geo;

namespace IIASA.Dataset.Gdal.Layer
{
    public class LayerData
    {
        public string Name { get; set; }
        public string GoemType { get; set; }
        public long FeatureCount { get; set; }
        public Extent Extent { get; set; }
        public string SpatialRefSystem { get; set; }
        public string ProjectionName { get; set; }
    }
}