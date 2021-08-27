using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace workshopdiomedes.Functions.Entities
{
    class ConsolidatedEntity: TableEntity
    {
        public int idemployee { get; set; }
        public DateTime date { get; set; }
        public int minutesWork { get; set; }
    }
}
