using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CandleStickMachineLearning.Models
{
    public class Bar
    {

        public double? Close { get; set; }

        public double? High { get; set; }

        public int? Volume { get; set; }

        public double? Open { get; set; }

        public double? Low { get; set; }


        public DateTime TimestampOffset { get; set; }

        public DateTime TimestampUtc { get; set; }

        public long EpochTimestamp { get; set; }
        [JsonIgnore]
        public Bar? PreviousBar { get; set; }
        [JsonIgnore]
        public Bar? NextBar { get; set; }
    }
}
