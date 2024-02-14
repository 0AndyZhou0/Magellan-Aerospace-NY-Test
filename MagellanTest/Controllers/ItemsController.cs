using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Xml.Linq;

namespace MagellanTest.Controllers
{
    public record Item
    {
        public int? Id { get; set; }

        public string? ItemName { get; set; }

        public int? ParentItem { get; set; }

        public int? Cost { get; set; }

        public string? ReqDate { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        // Create a new item record
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Item item)
        {
            var connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=Part";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            if (item.Id == null) {
                return BadRequest("id cannot be null");
            } else if (item.ItemName == null) {
                return BadRequest("item name cannot be null");
            } else if (item.Cost == null) {
                return BadRequest("cost cannot be null");
            } else if (item.ReqDate == null) {
                return BadRequest("req date cannot be null");
            }

            /*System.Diagnostics.Debug.WriteLine(String.Format("{0} {1} {2} {3} {4}", item.Id.ToString(), item.ItemName, item.ParentItem.ToString(), item.Cost.ToString(), item.ReqDate));*/

            await using var command = dataSource.CreateCommand(String.Format("INSERT INTO item\r\nVALUES \r\n({0}, '{1}', {2}, {3}, '{4}')",
                item.Id.ToString(), item.ItemName, item.ParentItem == null ? "NULL" : item.ParentItem.ToString(), item.Cost.ToString(), item.ReqDate));

            try
            {
                await command.ExecuteNonQueryAsync();
                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Get one item based on id
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItem(int id)
        {
            var connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=Part";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            await using (var cmd = dataSource.CreateCommand(String.Format("SELECT * \r\nFROM item \r\nWHERE id = {0}", id.ToString())))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    var item = new Item();
                    item.Id = reader.GetInt32(0);
                    item.ItemName = reader.GetString(1);
                    item.ParentItem = reader.IsDBNull(2) ? null : reader.GetInt32(2);
                    item.Cost = reader.GetInt32(3);
                    item.ReqDate = reader.GetDateTime(4).ToString("MM-dd-yyyy"); // Not sure if this should be determined by user culture
                    /*System.Diagnostics.Debug.WriteLine(String.Format("{0} {1} {2} {3} {4}", item.Id.ToString(), item.ItemName, item.ParentItem.ToString(), item.Cost.ToString(), item.ReqDate));*/
                    return Ok(item);
                }
            }

            return BadRequest(String.Format("'{0}' not found", id));

        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ItemsCostController : ControllerBase
    {
        // Get Total Cost function
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTotalCost(string name)
        {
            var connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=Part";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            await using (var cmd = dataSource.CreateCommand(String.Format("SELECT Get_Total_Cost('{0}')", name)))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        return BadRequest(String.Format("'{0}' doesn't exist or '{0}''s parent_item is not null", name));
                    }
                    var total = reader.GetInt32(0);
                    /*System.Diagnostics.Debug.WriteLine(String.Format("{0}", total));*/
                    return Ok(total);
                }
            }

            return BadRequest(String.Format("'{0}' not found", name));

        }
    }
}
