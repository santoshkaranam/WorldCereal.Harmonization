using System;
using IIASA.Dataset.Gdal.Geo;
using OSGeo.OGR;

namespace IIASA.Dataset.Gdal.Feature
{
    public class PointFeature : BaseFeature
    {
        public XY Geometry;

        [FieldMetaData(FieldType = FieldType.OFTString, Length = 32, Name = "imageryTime")]
        public DateTime? ImageryTime { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTString, Length = 32, Name = "validationTime")]
        public DateTime? ValidationTime { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTString, Length = 16, Name = "confidence")]
        public string Confidence { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "agreeingValidations")]
        public int AgreeingValidations { get; set; }
    }
}