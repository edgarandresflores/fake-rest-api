using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/legacy")]
public class LegacyController : ControllerBase
{
    private static List<Dictionary<string, object>> _legacy = new List<Dictionary<string, object>>();
    private static int _counter = 0;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_legacy);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_legacy.Find(l => (int)l["id"] == id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        _legacy.Add(payload);
        return Ok(payload);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var item = _legacy.Find(l => (int)l["id"] == id);
        if (item == null) return Ok("missing");
        foreach (var key in payload.Keys) item[key] = payload[key];
        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _legacy.RemoveAll(l => (int)l["id"] == id);
        return Ok(true);
    }

    [HttpGet("doStuff")]
    public IActionResult DoStuff(string? arg)
    {
        Thread.Sleep(5);
        return Ok("did:" + arg);
    }

    [HttpGet("convert/{id:int}")]
    public IActionResult ConvertThing(int id)
    {
        var item = _legacy.Find(l => (int)l["id"] == id);
        if (item == null) return Ok(false);
        item["converted"] = true;
        return Ok(item);
    }

    [HttpPost("migrate")]
    public IActionResult Migrate()
    {
        _legacy.Add(new Dictionary<string, object> { ["id"] = ++_counter, ["legacy"] = true });
        return Ok(_legacy.Count);
    }

    [HttpGet("search")]
    public IActionResult Search(string? q)
    {
        return Ok(_legacy.FindAll(l => l.ContainsKey("name") && l["name"].ToString()!.Contains(q ?? "")));
    }

    [HttpPost("dup")]
    public IActionResult Duplicate([FromBody] Dictionary<string, object> payload)
    {
        _legacy.Add(payload);
        _legacy.Add(payload);
        return Ok(_legacy.Count);
    }
}
