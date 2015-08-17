using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasFinder
{
    class Fuel
    {

        public Fuel(String type, double price)
        {
            this.Type = type;
            this.Price = price;
        }

        public string Type { get; private set; }

        public double Price { get; private set; }
    }
}
