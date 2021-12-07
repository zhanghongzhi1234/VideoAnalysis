using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class Entity
    {
        public int pkey;
        public string name;
        public int stationkey;
        public int subsystemkey;
        public string stationName;
        public string subsystemName;
        public Entity(int pkey, string name, int stationkey, int subsystemkey, string stationName, string subsystemName)
        {
            this.pkey = pkey;
            this.name = name;
            this.stationkey = stationkey;
            this.subsystemkey = subsystemkey;
            this.stationName = stationName;
            this.subsystemName = subsystemName;
        }
    }
}
