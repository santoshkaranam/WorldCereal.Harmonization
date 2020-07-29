namespace IIASA.Dataset.Gdal.Geo
{
    public class XY
    {
        public XY Initialize(string xyValue)
        {
            var split = xyValue.Split(":".ToCharArray());
            X = double.Parse(split[0]);
            Y = double.Parse(split[1]);
            return this;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }
}