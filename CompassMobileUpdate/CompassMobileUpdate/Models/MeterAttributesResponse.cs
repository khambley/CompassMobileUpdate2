using System;
namespace CompassMobileUpdate.Models
{
	public class MeterAttributesResponse
	{
        public string Form { get; set; }
        public string MacID { get; set; }
        public string WebID { get; set; }
        public string Status { get; set; }
        public DateTimeOffset StatusDate { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Version { get; set; }
        public string Model { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ManufacturerName { get; set; }
        public bool IsPingable { get; set; }
        public bool IsPQRCapable { get; set; }
    }
}

