using System;
using OSGeo.OGR;

namespace IIASA.Dataset.Gdal.Feature
{
    public class FieldMetaDataAttribute : Attribute
    {
        public string Name { get; set; }

        public int Length { get; set; }

        public FieldType FieldType { get; set; }
    }
}