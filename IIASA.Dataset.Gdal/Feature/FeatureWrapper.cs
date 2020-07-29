using System;
using System.Collections.Generic;
using System.Linq;
using IIASA.Dataset.Gdal.Geo;
using Newtonsoft.Json;
using OSGeo.OGR;
using SpatialReference = OSGeo.OSR.SpatialReference;

namespace IIASA.Dataset.Gdal.Feature
{
    public class FeatureWrapper
    {
        public OSGeo.OGR.Feature Value { get; }

        public FeatureWrapper(OSGeo.OGR.Feature feature)
        {
            Value = feature;
        }

        public string GetFieldAsString(string name)
        {
            //"CODE_CULTU"
            return Value.GetFieldAsString(name);
        }

        public void Set(string propName, string value)
        {
            Value.SetField(propName,value);
        }

        public void Set(string propName, DateTime value)
        {
            Value.SetField(propName, value.Year,value.Month,value.Day,0,0,0,0);
        }

        public void Set(string propName, int value)
        {
            Value.SetField(propName, value);
        }

        public void Set(string propName, double value)
        {
            Value.SetField(propName, value);
        }

        public void SetPolygonGeo(XY[] coordinates)
        {
            Geometry geometry = Geometry.CreateFromWkt($"POLYGON (({GetCoordinateString(coordinates)}))");
            Value.SetGeometry(geometry);
        }


        public void SetPointGeo(XY coordinate)
        {
            Geometry geometry = Geometry.CreateFromWkt($"POINT({coordinate.X} {coordinate.Y})");
            Value.SetGeometry(geometry);
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

        public PolygonFeatureGeoData GetFieldGeo()
        {
            var result = new PolygonFeatureGeoData();
            //"geom"
            var data = Value.GetGeometryRef();
            SpatialReference spatialReference = new SpatialReference(Constants.Epsg3035Wkt);
            data.TransformTo(spatialReference);
            
            result.Area = data.GetArea();
            var geomJsonString = data.ExportToJson(new string[0]);
            var geoData = JsonConvert.DeserializeObject<GeometryData>(geomJsonString);
            var listOfCoordinates = new List<XY>();
            foreach (var coordinate in geoData.Coordinates)
            {
                // for polygon type.
                foreach (var innerCoordinate in coordinate)
                {
                    var xy = new XY {X = innerCoordinate.First(), Y = innerCoordinate.Last()};
                    listOfCoordinates.Add(xy);
                }
            }

            result.Coordinates = listOfCoordinates;

            return result;
        }

        public double GetArea()
        {
            var data = Value.GetGeometryRef();
            if (data == null)
            {
                return 0.0;
            }
            SpatialReference spatialReference = new SpatialReference(Constants.Epsg3035Wkt);
            data.TransformTo(spatialReference);

            return data.GetArea();
        }
    }
}