using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class AlarmNumber
    {
        public int pkey;
        public int stationkey;
        public int powerValue;
        public int alarmNumber;
        public string stationName;
        public AlarmNumber(int pkey, int stationkey, int powerValue, int alarmNumber, string stationName)
        {
            this.pkey = pkey;
            this.stationkey = stationkey;
            this.powerValue = powerValue;
            this.alarmNumber = alarmNumber;
            this.stationName = stationName;
        }
    }
}