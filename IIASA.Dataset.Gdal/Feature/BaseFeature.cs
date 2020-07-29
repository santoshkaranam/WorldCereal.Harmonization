using System;
using OSGeo.OGR;

namespace IIASA.Dataset.Gdal.Feature
{
    public abstract class BaseFeature
    {
        [FieldMetaData(FieldType = FieldType.OFTString, Length = 64, Name = "sampleID")]
        public string SampleId { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTDateTime, Length = 32, Name = "date")]
        public DateTime? ValidityTime { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "userConf")]
        public int UserConf { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "LC")]
        public int LandCover { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "CT1")]
        public int CropType1 { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "CT2")]
        public int CropType2 { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "irr1")]
        public int Irrigation1 { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "irr2")]
        public int Irrigation2 { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTInteger, Length = 32, Name = "irr3")]
        public int Irrigation3 { get; set; }

        [FieldMetaData(FieldType = FieldType.OFTString, Length = 64, Name = "extendedData")]
        public string ExtendedData { get; set; } // for tracability, will be removed if not required.
    }
}