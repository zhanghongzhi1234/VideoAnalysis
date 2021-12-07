using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class Config
    {
        public int pkey;
        public string name;
        public string value;
        public Config(int pkey, string name, string value)
        {
            this.pkey = pkey;
            this.name = name;
            this.value = value;
        }
    }
}
