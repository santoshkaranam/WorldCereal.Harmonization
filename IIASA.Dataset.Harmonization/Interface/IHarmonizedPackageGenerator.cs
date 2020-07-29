using System;

namespace IIASA.Dataset.Harmonization.Interface
{
    public interface IHarmonizedPackageGenerator
    {
        void Generate(string shpPath, string csvPath, DateTime dateTime, string countryCode);
    }
}