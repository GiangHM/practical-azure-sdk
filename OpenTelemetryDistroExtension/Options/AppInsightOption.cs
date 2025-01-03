using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTelemetryDistroExtension.Options
{
    public class AppInsightOption
    {
        public string ClientId { get; set; }
        public string AIRoleName { get; set; }
        public  bool IsUseSampling { get; set; }
        public float FixSampling { get; set; }
        public bool LiveMetric { get; set; } = true;
    }
}
