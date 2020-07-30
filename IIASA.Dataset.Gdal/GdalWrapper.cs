using System;
using System.Collections.Generic;
using IIASA.Dataset.Gdal.Geo;
using IIASA.Dataset.Gdal.Layer;
using MaxRev.Gdal.Core;
using OSGeo.OGR;
using SpatialReference = OSGeo.OSR.SpatialReference;

namespace IIASA.Dataset.Gdal
{
    // generic wrapper for both geopackage and geodatabase.
    public class GdalWrapper : IDisposable
    {
          private DataSource _dataSource;

        public void Configure()
        {
            GdalBase.ConfigureAll();
        }

        public void InitializeDatasetForRead(string path, string driverName)
        {
            var driver = Ogr.GetDriverByName(driverName);
            _dataSource = driver.Open(path, 0);
        }

        public void InitializeDatasetForCreate(string path, string driverName)
        {
            Ogr.RegisterAll();
            Driver driverSH = Ogr.GetDriverByName(driverName);

            if (driverSH == null)
            {
                Console.WriteLine("Cannot get drivers. Exiting");
                Environment.Exit(-1);
            }

            Console.WriteLine("Drivers fetched");

            // Creating a GeoPackage
            _dataSource = driverSH.CreateDataSource(path, new string[] { });
            if (_dataSource == null)
            {
                Console.WriteLine("Cannot create datasource");
                Environment.Exit(-1);
            }
        }


        public IEnumerable<LayerWrapper> GetLayers()
        {
            if (_dataSource == null)
            {
                throw new Exception("Initialize DataSource from InitializeDatasetForRead() method");
            }

            var result = new List<LayerWrapper>();

            var count = _dataSource.GetLayerCount();
            for (int i = 0; i < count; i++)
            {
                var ogrLayer = _dataSource.GetLayerByIndex(i);
                var layerWrapper = new LayerWrapper(ogrLayer);  
                result.Add(layerWrapper);
            }

            return result;
        }

        public LayerWrapper CreateWGS84PolygonLayer(string name)
        {
            return Wgs84Layer(name, wkbGeometryType.wkbPolygon);
        }

        public LayerWrapper CreateWGS84PointLayer(string name)
        {
            return Wgs84Layer(name, wkbGeometryType.wkbPoint);
        }

        private LayerWrapper Wgs84Layer(string name, wkbGeometryType wkbGeometryType)
        {
            OSGeo.OGR.Layer layerSH;
            SpatialReference spatialReference = new SpatialReference(Constants.Epsg4326Wkt);
            layerSH = _dataSource.CreateLayer(name, spatialReference, wkbGeometryType, new string[] { });
            if (layerSH == null)
            {
                Console.WriteLine("Layer creation failed, exiting...");
                return null;
            }

            Console.WriteLine($"{wkbGeometryType} Layer created");
            return new LayerWrapper(layerSH);
        }


        private Extent GetExtent(OSGeo.OGR.Layer ogrLayer)
        {
            var envelope = new Envelope();
            ogrLayer.GetExtent(envelope, 0);
            return new Extent {MaxX = envelope.MaxX, MaxY = envelope.MaxY, MinX = envelope.MinX, MinY = envelope.MinY};
        }

        public void Flush()
        {
            _dataSource?.FlushCache();
        }

        public void Dispose()
        {
            _dataSource?.FlushCache();
            _dataSource?.Dispose();
        }
    }
}
