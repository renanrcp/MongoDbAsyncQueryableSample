using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDbAsyncQueryableSample.Database;
using MongoDbAsyncQueryableSample.Models;

namespace MongoDbAsyncQueryableSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly INoSqlCollection<Product> _productsCollection;

        public ProductController(INoSqlDbFactory dbFactory)
        {
            var db = dbFactory.Create("test");

            _productsCollection = db.GetCollection<Product>("products");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var products = await _productsCollection
                                    .AsQueryable()
                                    .Where(a => a.ProductId == 1)
                                    .ToListAsync();

            return Ok(products);
        }
    }
}