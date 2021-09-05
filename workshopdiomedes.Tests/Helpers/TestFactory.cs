using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using workshopdiomedes.Common.Models;
using workshopdiomedes.Functions.Entities;

namespace workshopdiomedes.Tests.Helpers
{
    public class TestFactory
    {
        public static List<WorkshopEntity> GetAllWorkshopEntity()
        {
            List<WorkshopEntity> lista = new List<WorkshopEntity>();
            WorkshopEntity time =new WorkshopEntity
            {
                ETag = "*",
                PartitionKey = "WORKSHOP",
                RowKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                consolidated = false,
                date = DateTime.UtcNow,
                idemployee=7,
                type=0,
            };
            lista.Add(time);
            return lista;
        }

        public static WorkshopEntity GetWorkshopEntity()
        {
            return new WorkshopEntity
            {
                ETag = "*",
                PartitionKey = "WORKSHOP",
                RowKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                consolidated = false,
                date = DateTime.UtcNow,
                idemployee = 7,
                type = 0,
            };
        }

        public static List<ConsolidatedEntity> GetAllConsolidatedEntity()
        {
            List<ConsolidatedEntity> lista = new List<ConsolidatedEntity>();
            ConsolidatedEntity consolidated = new ConsolidatedEntity
            {
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                date = DateTime.UtcNow,
                idemployee = 7,
                minutesWork=20,
            };
            lista.Add(consolidated);
            return lista;
        }

        public static ConsolidatedEntity GetConsolidatedEntity()
        {
            return new ConsolidatedEntity
            {
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                date = DateTime.UtcNow,
                idemployee = 7,
                minutesWork = 20,
            };
        }


        public static DefaultHttpRequest CreateHttpRequest(Guid workshopId, Workshop workshopRequest)
        {
            string request = JsonConvert.SerializeObject(workshopRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{workshopId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(DateTime date)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{date}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid workshopId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{workshopId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Workshop workshopRequest)
        {
            string request = JsonConvert.SerializeObject(workshopRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Workshop GetWorkshopRequest()
        {
            return new Workshop
            {
                date=DateTime.UtcNow,
                consolidated=false,
                idemployee=7,
                type=0
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}
