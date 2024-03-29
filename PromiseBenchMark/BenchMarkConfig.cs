using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromiseBenchMark
{
    public class BenchMarkConfig : ManualConfig
    {
        public BenchMarkConfig()
        {
            //AddExporter(CsvMeasurementsExporter.Default);
            //AddExporter(RPlotExporter.Default);
        }
    }
}
