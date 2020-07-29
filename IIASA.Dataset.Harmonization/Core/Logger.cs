using System;
using IIASA.Dataset.Harmonization.Interface;

namespace IIASA.Dataset.Harmonization.Core
{
    public class Logger : ILogger
    {
        public void Line(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}