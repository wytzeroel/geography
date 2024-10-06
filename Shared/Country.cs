using Azure;
using Azure.Data.Tables;

using System;

namespace BlazorApp.Shared
{
    public class Country : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string Name { get; set; }
        public string Capital { get; set; }
        public int Population { get; set; }
        public float Area { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string FlagUrl() => $"http://127.0.0.1:10000/devstoreaccount1/flags/{RowKey}";
    }
}
