using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class PowerArchive
    {
        public int pkey;
        public int stationkey;
        public int subsystemkey;
        public int yearindex;
        public int monthindex;
        public int value;

        public PowerArchive(int pkey, int stationkey, int subsystemkey, int yearindex, int monthindex, int value)
        {
            this.pkey = pkey;
            this.stationkey = stationkey;
            this.subsystemkey = subsystemkey;
            this.yearindex = yearindex;
            this.monthindex = monthindex;
            this.value = value;
        }
    }
}
