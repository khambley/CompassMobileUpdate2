using System;
namespace CompassMobileUpdate.Models
{
	public class MeterReadsResponse
	{
        public decimal? VoltagePhaseA { get; set; }
        public decimal? VoltagePhaseB { get; set; }
        public decimal? VoltagePhaseC { get; set; }
        public bool AreAllVoltagesValid { get; set; }
        public MeterReadsResponse()
		{
		}
	}
}

