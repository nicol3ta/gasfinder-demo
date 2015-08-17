using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasFinder
{
    class GasStation
    {
        public GasStation(String title, double latitude, double longitude, Fuel[] fuelTypes)
        {
            this.Title = title;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.FuelTypes = fuelTypes;
        }

        public string Title { get; private set; }

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public Fuel[] FuelTypes { get; private set; }
    }

    
}
