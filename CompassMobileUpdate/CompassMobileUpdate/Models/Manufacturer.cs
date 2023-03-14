using System;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.Models
{
    public class Manufacturer
    {

        private string _meterImagePath;
        private string _manufactuerLetter;
        private string _name;
        private Manufacturers _manufacturerType;


        #region Public Properties
        public string MeterImagePath { get { return _meterImagePath; } }
        public string ManufacturerLeter { get { return _manufactuerLetter; } }
        public string Name { get { return _name; } }
        public Manufacturers ManufacturerType { get { return _manufacturerType; } }
        public string UIQ_ManufacturerName { get; set; }

        #endregion //Public Properties

        #region Constructors
        public Manufacturer(string manufacturerLetter)
        {
            _manufactuerLetter = manufacturerLetter;
            _meterImagePath = GetManufacturerMeterImagePath(manufacturerLetter);
            _manufacturerType = GetManufacturerFromLetter(manufacturerLetter);
            _name = GetManufacturerShortName(_manufacturerType);
        }
        #endregion

        #region HelperMethods
        /// <summary>
        /// Pass one letter to get the Manufacturer
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        private static Manufacturers GetManufacturerFromLetter(string letter)
        {
            letter = letter.ToUpper();
            if (letter.Equals("G"))
                return Manufacturers.GE;
            else if (letter.Equals("E"))
                return Manufacturers.Elster;
            else if (letter.Equals("D"))
                return Manufacturers.LandG;
            else
                return Manufacturers.Unknown;
        }
        /// <summary>
        /// Returns the short name used to identify a manufacturer
        /// </summary>
        /// <param name="ms"></param>
        private string GetManufacturerShortName(Manufacturers ms)
        {
            if (ms == Manufacturers.GE)
                return "GE";
            else if (ms == Manufacturers.Elster)
                return "Elster";
            else if (ms == Manufacturers.LandG)
                return "L&G";
            else if (ms == Manufacturers.Unknown)
                return "Unknown";
            else
                return "Unknown";
        }

        /// <summary>
        /// Returns the short name used to identify a manufacturer
        /// </summary>
        /// <param name="ms"></param>
        private string GetManufacturerMeterImagePath(Manufacturers ms)
        {
            string prefix = "meter";
            string postfix = "_" + GetManufacturerImagePostfix(ms);
            string imageType = ".png";

            return prefix + postfix + imageType;
        }

        /// <summary>
        /// Returns the single letter code used to identify a manufacturer's meter_x.png image
        /// </summary>
        /// <param name="ms"></param>
        private string GetManufacturerImagePostfix(Manufacturers ms)
        {
            if (ms == Manufacturers.GE)
                return "g";
            if (ms == Manufacturers.Elster)
                return "default";//return "e"; --uncomment when you have the meter image added
            else if (ms == Manufacturers.LandG)
                return "default";//return "l"; --uncomment when you have the meter image added
            else if (ms == Manufacturers.Unknown)
                return "default";
            else
                return "default";
        }

        /// <summary>
        /// Returns the name of the Image for this particular Meter
        /// </summary>
        /// <param name="manufacturerLetter"></param>
        /// <returns></returns>
        private string GetManufacturerMeterImagePath(string manufacturerLetter)
        {
            Manufacturers ms = GetManufacturerFromLetter(manufacturerLetter);
            return GetManufacturerMeterImagePath(ms);
        }

        #endregion //helper methods
    }
}

