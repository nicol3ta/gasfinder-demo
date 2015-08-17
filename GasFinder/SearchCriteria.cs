using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasFinder
{
    public enum Criteria
    {
        Distance,
        Price
    }
    public enum FuelType
    {
        E10,
        Super,
        SuperPlus,
        Diesel
    }

    public class SearchCriteria
    {
        public SearchCriteria(Criteria criteria, FuelType fuel)
        {
            this.criteria = criteria;
            this.fuel = fuel;
        }

        public Criteria criteria;
        public FuelType fuel;
    }
}
