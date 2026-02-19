using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/invoices")]
public class InvoicesController : ControllerBase
{
    private static List<Dictionary<string, object>> _invoices = new List<Dictionary<string, object>>();
    private static int _counter = 0;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_invoices);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_invoices.Find(i => (int)i["id"] == id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        payload["created"] = DateTime.Now;
        _invoices.Add(payload);
        return Ok(payload);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var invoice = _invoices.Find(i => (int)i["id"] == id);
        if (invoice == null) return Ok("missing");
        foreach (var key in payload.Keys) invoice[key] = payload[key];
        return Ok(invoice);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _invoices.RemoveAll(i => (int)i["id"] == id);
        return Ok(true);
    }

    [HttpGet("generate")]
    public IActionResult Generate(string? customer)
    {
        var invoice = new Dictionary<string, object> { ["id"] = ++_counter, ["customer"] = customer ?? "na" };
        _invoices.Add(invoice);
        return Ok(invoice);
    }

    [HttpPost("{id:int}/email")]
    public IActionResult Email(int id, string? to)
    {
        var invoice = _invoices.Find(i => (int)i["id"] == id);
        if (invoice == null) return Ok("missing");
        invoice["emailed"] = to ?? "unknown";
        return Ok(invoice);
    }

    [HttpGet("search")]
    public IActionResult Search(string? q)
    {
        return Ok(_invoices.FindAll(i => i["customer"].ToString()!.Contains(q ?? "")));
    }

    [HttpPost("bulk")]
    public IActionResult Bulk([FromBody] List<Dictionary<string, object>> payload)
    {
        foreach (var item in payload)
        {
            item["id"] = ++_counter;
            _invoices.Add(item);
        }
        return Ok(payload.Count);
    }

    [HttpGet("export")]
    public IActionResult Export()
    {
        Thread.Sleep(5);
        return Ok(_invoices.Select(i => i["id"]).ToList());
    }
}
