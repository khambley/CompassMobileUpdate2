using System;
namespace CompassMobileUpdate.Models
{
	public class VoltageRule
	{
        public int ID { get; set; }

        public string MeterForm { get; set; }

        public string MeterType { get; set; }

        public int TargetVoltage { get; set; }

        public int ResidentialVoltageLow { get; set; }

        public int ResidentialVoltageHigh { get; set; }

        public int CommercialVoltageLow { get; set; }

        public int CommercialVoltageHigh { get; set; }

        public string PhaseType { get; set; }

        public bool OverlappingVoltages { get; set; }
    }
}

