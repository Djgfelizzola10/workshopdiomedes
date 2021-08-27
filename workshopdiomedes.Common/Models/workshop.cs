using System;
using System.Collections.Generic;
using System.Text;

namespace workshopdiomedes.Common.Models
{
    public class workshop
    {
        public int idemployee { get; set; }
        public DateTime date { get; set; }
        public int type { get; set; }
        public Boolean consolidated { get; set; }
    }
}
