using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace workshopdiomedes.Functions.Entities
{
    public class ConsolidatedEntity : TableEntity
    {
        public int idemployee { get; set; }
        public DateTime date { get; set; }
        public int minutesWork { get; set; }

        public static implicit operator ConsolidatedEntity(WorkshopEntity v)
        {
            throw new NotImplementedException();
        }
    }
}
