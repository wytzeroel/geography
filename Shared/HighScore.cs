using Azure;
using Azure.Data.Tables;

using System;

namespace BlazorApp.Shared
{
    public class HighScore : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string Name { get; set; }
        public int Score { get; set; }
        public string Game { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public HighScore()
        {
        }

        public HighScore(string Name, int Score, string Game)
        {
            PartitionKey = Game;
            RowKey = Name;

            this.Name = Name;
            this.Score = Score;
            this.Game = Game;
        }

        public void setName(string name)
        {
            this.Name = name;
            this.RowKey = name;
        }
    }
}