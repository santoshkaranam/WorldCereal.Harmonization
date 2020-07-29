using System;
using System.Collections.Generic;
using IIASA.Dataset.Gdal.Feature;
using IIASA.Dataset.Gdal.Geo;
using OSGeo.OGR;

namespace IIASA.Dataset.Gdal.Layer
{
    public class LayerWrapper:IDisposable
    {
        private readonly OSGeo.OGR.Layer _ogrLayer;

        public LayerWrapper(OSGeo.OGR.Layer ogrLayer)
        {
            _ogrLayer = ogrLayer;
        }

        public bool CreateField(string name, FieldType fieldType, int length)
        {
            // Creating and adding attribute fields to layer
            FieldDefn fieldDefn = new FieldDefn(name, fieldType);
            fieldDefn.SetWidth(length);

            if (_ogrLayer.CreateField(fieldDefn, 1) != 0)
            {
                Console.WriteLine("Creating field failed");
                return false;
            }

            return true;
        }

        public void WriteToDisk()
        {
            _ogrLayer.SyncToDisk();
        }

        public LayerData GetData()
        {
            var spatialReference = _ogrLayer.GetSpatialRef();
            spatialReference.ExportToPrettyWkt(out string spref, 0);
            var layer = new LayerData
            {
                Name = _ogrLayer.GetName(),
                GoemType = _ogrLayer.GetGeomType().ToString("G").Substring(3),
                FeatureCount = _ogrLayer.GetFeatureCount(0),
                Extent = GetExtent(_ogrLayer),
                SpatialRefSystem = spref,
                ProjectionName = spatialReference.GetName()
            };

            return layer;
        }

        private Extent GetExtent(OSGeo.OGR.Layer ogrLayer)
        {
            var envelope = new Envelope();
            ogrLayer.GetExtent(envelope, 0);
            return new Extent { MaxX = envelope.MaxX, MaxY = envelope.MaxY, MinX = envelope.MinX, MinY = envelope.MinY };
        }


        public void AddValueToField(PointFeature featureData)
        {
            OSGeo.OGR.Feature feature = new OSGeo.OGR.Feature(_ogrLayer.GetLayerDefn());

            feature.SetField(nameof(featureData.SampleId), featureData.SampleId);
            feature.SetField(nameof(featureData.Confidence), featureData.Confidence);
            feature.SetField(nameof(featureData.AgreeingValidations), featureData.AgreeingValidations);
            feature.SetField(nameof(featureData.UserConf), featureData.UserConf);

            feature.SetField(nameof(featureData.ValidityTime),
                featureData.ValidityTime != null ? featureData.ValidityTime.Value.ToString("yyyy-MM-dd") : "");

            feature.SetField(nameof(featureData.ImageryTime),
                featureData.ImageryTime != null ? featureData.ImageryTime.Value.ToString("yyyy-MM-dd") : "");

            feature.SetField(nameof(featureData.ValidationTime),
                featureData.ValidationTime != null ? featureData.ValidationTime.Value.ToString("yyyy-MM-dd") : "");

            feature.SetField(nameof(featureData.LandCover), featureData.LandCover);
            feature.SetField(nameof(featureData.CropType1), featureData.CropType1);
            feature.SetField(nameof(featureData.CropType2), featureData.CropType2);
            feature.SetField(nameof(featureData.Irrigation1), featureData.Irrigation1);
            feature.SetField(nameof(featureData.Irrigation2), featureData.Irrigation2);
            feature.SetField(nameof(featureData.Irrigation3), featureData.Irrigation3);
            feature.SetField(nameof(featureData.ExtendedData), featureData.ExtendedData);

            Geometry geometry = Geometry.CreateFromWkt($"POINT({featureData.Geometry.X} {featureData.Geometry.Y})");
            feature.SetGeometry( geometry);

            _ogrLayer.CreateFeature(feature);
        }

        public void AddValueToField(PolygonFeature featureData)
        {
            OSGeo.OGR.Feature feature = new OSGeo.OGR.Feature(_ogrLayer.GetLayerDefn());
            feature.SetField(nameof(featureData.SampleId), featureData.SampleId);
            feature.SetField(nameof(featureData.Area), featureData.Area);
            feature.SetField(nameof(featureData.LandCover), featureData.LandCover);
            feature.SetField(nameof(featureData.CropType1), featureData.CropType1);
            feature.SetField(nameof(featureData.CropType2), featureData.CropType2);
            feature.SetField(nameof(featureData.Irrigation1), featureData.Irrigation1);
            feature.SetField(nameof(featureData.Irrigation2), featureData.Irrigation2);
            feature.SetField(nameof(featureData.Irrigation3), featureData.Irrigation3);
            feature.SetField(nameof(featureData.ExtendedData), featureData.ExtendedData);
            feature.SetField(nameof(featureData.ValidityTime),
                featureData.ValidityTime != null ? featureData.ValidityTime.Value.ToString("yyyy-MM-dd") : "");
            Geometry geometry = Geometry.CreateFromWkt($"POLYGON (({GetCoordinateString(featureData.Geometry)}))");
            feature.SetGeometry(geometry);

            _ogrLayer.CreateFeature(feature);
        }

        public FeatureWrapper CreateFeature()
        {
            return new FeatureWrapper(new OSGeo.OGR.Feature(_ogrLayer.GetLayerDefn()));
        }

        public void AddFeatureToLayer(FeatureWrapper feature)
        {
            _ogrLayer.CreateFeature(feature.Value);
        }

        private string GetCoordinateString(XY[] featureDataGeometry)
        {
            var builder = new List<string>();
            foreach (var xy in featureDataGeometry)
            {
                builder.Add($"{xy.Y} {xy.X}");
            }

            return string.Join(", ", builder.ToArray());
        }

        public void Dispose()
        {
            _ogrLayer?.Dispose();
        }

        public FeatureWrapper GetNextFeature()
        {
            var feature = _ogrLayer.GetNextFeature();
            if (feature == null)
            {
                return null;
            }

            var featureWrapper = new FeatureWrapper(feature);
            return featureWrapper;
        }

        public FeatureWrapper GetFeature(long index)
        {
            try
            {
                var feature = _ogrLayer.GetFeature(index);
                if (feature == null)
                {
                    return null;
                }

                var featureWrapper = new FeatureWrapper(feature);
                return featureWrapper;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}