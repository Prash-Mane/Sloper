using System;
using Xamarin.Forms.GoogleMaps;

namespace SloperMobile
{
    public class LocationModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }


        public Position ToPosition() => new Position(Latitude, Longitude);
    }
}
