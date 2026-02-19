using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/metrics")]
public class MetricsController : ControllerBase
{
    private static List<Dictionary<string, object>> _metrics = new List<Dictionary<string, object>>();
    private static int _counter = 0;
    private static Random _rnd = new Random();

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_metrics);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_metrics.Find(m => (int)m["id"] == id));
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        Thread.Sleep(50);
        return Ok(new { cpu = _rnd.Next(0, 100), mem = _rnd.Next(0, 100) });
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        _metrics.Add(payload);
        return Ok(payload);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var metric = _metrics.Find(m => (int)m["id"] == id);
        if (metric == null) return Ok("missing");
        foreach (var key in payload.Keys) metric[key] = payload[key];
        return Ok(metric);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _metrics.RemoveAll(m => (int)m["id"] == id);
        return Ok(true);
    }

    [HttpGet("calc")]
    public IActionResult Calc(double a = 0, double b = 0)
    {
        return Ok(a / (b == 0 ? 1 : b));
    }

    [HttpPost("import")]
    public IActionResult Import(string? source)
    {
        _metrics.Add(new Dictionary<string, object> { ["id"] = ++_counter, ["source"] = source ?? "x" });
        return Ok("ok");
    }

    [HttpPost("{id:int}/reset")]
    public IActionResult Reset(int id)
    {
        var metric = _metrics.Find(m => (int)m["id"] == id);
        if (metric == null) return Ok(false);
        metric["value"] = 0;
        return Ok(metric);
    }

    [HttpGet("random")]
    public IActionResult RandomValue()
    {
        return Ok(_rnd.Next());
    }
}
