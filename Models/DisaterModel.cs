using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace disaterprediction.Models
{
    public class RegionModel
    {
        public string RegionID { get; set; }

        public LatLong LocationCoordinates { get; set; }

        public string[] DisaterTypes { get; set; }
    }

    public class LatLong
    {
        public decimal latitude { get; set; }
        public decimal longtitude { get; set; }
    }

    public class AlertSetting
    {
        public string RegionID { get; set; }
        public string DisaterTypes { get; set; }

        public int ThresholdScore { get; set; }
    }

    public class DisaterRisk
    {
        public string RegionID { get; set; }
        public string DisaterTypes { get; set; }

        public int RiskScore { get; set; }

        public string RiskLevel { get; set; }

        public string AlertTriggered { get; set; }
    }

    public class responseAPI
    {
        public string type { get; set; }
        public metadata metadata { get; set; }

        public List<features> features { get; set; }
    }

    public class metadata
    {
        public long generated { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string api { get; set; }
        public int count { get; set; }
        public int status { get; set; }
    }

    public class features
    {
        public string type { get; set; }
        public properties properties { get; set; }

        public geometry geometry { get; set; }

        public string id { get; set; }
    }

    public class properties
    {
        public decimal mag { get; set; }
        public string place { get; set; }
        public long time { get; set; }
        public long updated { get; set; }
        public int tz { get; set; }
        public string url { get; set; }
        public string detail { get; set; }
        public int felt { get; set; }
        public decimal cdi { get; set; }
        public string alert { get; set; }
        public string status { get; set; }
        public string tsunami { get; set; }
        public int sig { get; set; }
        public string net { get; set; }
        public string code { get; set; }
        public string ids { get; set; }
        public string sources { get; set; }
        public string types { get; set; }
        public int nst { get; set; }
        public decimal dmin { get; set; }

        public decimal sms { get; set; }
        public decimal gap { get; set; }

        public string magType { get; set; }
        public string type { get; set; }
        public string title { get; set; }
    }

    public class geometry
    {
        public string type { get; set; }

        public decimal[] DisaterTypes { get; set; }
    }

}