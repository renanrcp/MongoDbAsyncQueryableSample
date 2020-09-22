using System;

namespace MongoDbAsyncQueryableSample.Models
{
    public class Product
    {
        public string Name { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}