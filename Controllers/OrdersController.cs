using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private static List<Dictionary<string, object>> _orders = new List<Dictionary<string, object>>();
    private static int _counter = 0;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_orders);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_orders.Find(o => (int)o["id"] == id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        payload["status"] = "new";
        _orders.Add(payload);
        return Ok(payload);
    }

    [HttpGet("create")]
    public IActionResult CreateViaGet(string? note)
    {
        _orders.Add(new Dictionary<string, object> { ["id"] = ++_counter, ["note"] = note ?? "" });
        return Ok(_counter);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var order = _orders.Find(o => (int)o["id"] == id);
        if (order == null) return Ok("missing");
        foreach (var key in payload.Keys) order[key] = payload[key];
        return Ok(order);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _orders.RemoveAll(o => (int)o["id"] == id);
        return Ok("deleted");
    }

    [HttpPost("{id:int}/approve")]
    public IActionResult Approve(int id)
    {
        var order = _orders.Find(o => (int)o["id"] == id);
        if (order == null) return Ok(false);
        order["status"] = "approved";
        return Ok(order);
    }

    [HttpPost("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var order = _orders.Find(o => (int)o["id"] == id);
        if (order == null) return Ok(false);
        order["status"] = "cancelled";
        return Ok(order);
    }

    [HttpGet("status/{id:int}")]
    public IActionResult Status(int id)
    {
        var order = _orders.Find(o => (int)o["id"] == id);
        return Ok(order == null ? "unknown" : order["status"]);
    }

    [HttpPost("bulkstatus")]
    public IActionResult BulkStatus([FromBody] List<int> ids)
    {
        foreach (var id in ids)
        {
            var order = _orders.Find(o => (int)o["id"] == id);
            if (order != null) order["status"] = "bulk";
        }
        return Ok(ids.Count);
    }
}
