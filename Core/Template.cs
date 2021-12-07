using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class Template
    {
        public int pkey;
        public string name;
        public string type;
        public string note;
        public Template(int pkey, string name, string type, string note)
        {
            this.pkey = pkey;
            this.name = name;
            this.type = type;
            this.note = note;
        }
    }
}
