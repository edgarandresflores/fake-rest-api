using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private static List<Dictionary<string, object>> _payments = new List<Dictionary<string, object>>();
    private static int _counter = 0;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_payments);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_payments.Find(p => (int)p["id"] == id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        payload["status"] = "paid";
        _payments.Add(payload);
        return Ok(payload);
    }

    [HttpGet("charge")]
    public IActionResult ChargeViaGet(string? card, double amount = 0)
    {
        _payments.Add(new Dictionary<string, object> { ["id"] = ++_counter, ["card"] = card ?? "na", ["amount"] = amount });
        return Ok(true);
    }

    [HttpPost("{id:int}/refund")]
    public IActionResult Refund(int id)
    {
        var p = _payments.Find(x => (int)x["id"] == id);
        if (p == null) return Ok("missing");
        p["status"] = "refunded";
        return Ok(p);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var p = _payments.Find(x => (int)x["id"] == id);
        if (p == null) return Ok("not found");
        foreach (var key in payload.Keys) p[key] = payload[key];
        return Ok(p);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _payments.RemoveAll(x => (int)x["id"] == id);
        return Ok("deleted");
    }

    [HttpGet("stats")]
    public IActionResult Stats()
    {
        var total = _payments.Sum(x => x.ContainsKey("amount") ? Convert.ToDouble(x["amount"]) : 0);
        return Ok(new { total, count = _payments.Count });
    }

    [HttpPost("import")]
    public IActionResult Import(string? source)
    {
        Thread.Sleep(5);
        _payments.Add(new Dictionary<string, object> { ["id"] = ++_counter, ["source"] = source ?? "x" });
        return Ok("ok");
    }

    [HttpPost("retry/{id:int}")]
    public IActionResult Retry(int id)
    {
        var p = _payments.Find(x => (int)x["id"] == id);
        if (p == null) return Ok(false);
        p["status"] = "retry";
        return Ok(p);
    }
}
