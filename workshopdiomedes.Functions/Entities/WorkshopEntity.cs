using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace workshopdiomedes.Functions.Entities
{
    internal class WorkshopEntity : TableEntity
    {
        public int idemployee { get; set; }
        public DateTime date { get; set; }
        public int type { get; set; }
        public bool consolidated { get; set; }
    }
}
