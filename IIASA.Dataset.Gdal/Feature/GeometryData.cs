using System.Collections.Generic;

namespace IIASA.Dataset.Gdal.Feature
{
    public class GeometryData
    {
        public string Type { get; set; }

        public List<List<List<double>>> Coordinates { get; set; }
    }
}