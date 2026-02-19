using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private static List<Dictionary<string, object>> _products = new List<Dictionary<string, object>>();
    private static int _counter = 0;
    private static Random _rnd = new Random();

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_products.Count == 0 ? "empty" : _products);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_products.Find(p => (int)p["id"] == id));
    }

    [HttpGet("search")]
    public IActionResult Search(string? q)
    {
        var result = _products.FindAll(p => p["name"].ToString()!.Contains(q ?? ""));
        return Ok(result);
    }

    [HttpGet("price/{id:int}")]
    public IActionResult GetPrice(int id)
    {
        var p = _products.Find(x => (int)x["id"] == id);
        return Ok(p == null ? "0" : p["price"].ToString());
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        payload["created"] = DateTime.Now;
        _products.Add(payload);
        return Ok(payload);
    }

    [HttpPost("{id:int}/discount")]
    public IActionResult Discount(int id, double percent = 0)
    {
        var p = _products.Find(x => (int)x["id"] == id);
        if (p == null) return Ok("missing");
        p["price"] = Convert.ToDouble(p["price"]) * (1 - (percent / 100));
        return Ok(p);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var p = _products.Find(x => (int)x["id"] == id);
        if (p == null) return Ok("not found");
        foreach (var key in payload.Keys) p[key] = payload[key];
        return Ok(p);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _products.RemoveAll(x => (int)x["id"] == id);
        return Ok(true);
    }

    [HttpPost("bulk")]
    public IActionResult Bulk([FromBody] List<Dictionary<string, object>> payload)
    {
        foreach (var item in payload)
        {
            item["id"] = ++_counter;
            _products.Add(item);
        }
        return Ok(payload.Count);
    }

    [HttpGet("legacylist")]
    public IActionResult LegacyList()
    {
        Thread.Sleep(5);
        return Ok(_products.Select(p => p["name"]).ToList());
    }
}
