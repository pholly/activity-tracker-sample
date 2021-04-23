using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pholly.Sample.Shared
{
    internal class ActivityStat
    {
        public string Action { get; set; }
        public int TotalTimeInSeconds { get; set; }
        public int Count { get; set; }
        public double AvgTimeInSeconds => Count > 0 ? TotalTimeInSeconds / Count : 0;
    }
}
