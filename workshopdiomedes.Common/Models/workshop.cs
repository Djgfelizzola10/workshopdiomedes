using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace workshopdiomedes.Common.Models
{
    public class Workshop
    {
        public int idemployee { get; set; }
        public DateTime date { get; set; }
        public int type { get; set; }
        public Boolean consolidated { get; set; }
    }
}
