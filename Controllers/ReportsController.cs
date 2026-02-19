using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private static List<Dictionary<string, object>> _reports = new List<Dictionary<string, object>>();
    private static int _counter = 0;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_reports);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_reports.Find(r => (int)r["id"] == id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        payload["created"] = DateTime.Now;
        _reports.Add(payload);
        return Ok(payload);
    }

    [HttpPost("{id:int}/run")]
    public IActionResult Run(int id)
    {
        var report = _reports.Find(r => (int)r["id"] == id);
        if (report == null) return Ok("missing");
        report["lastRun"] = DateTime.Now;
        return Ok(report);
    }

    [HttpGet("run/{id:int}")]
    public IActionResult RunViaGet(int id)
    {
        var report = _reports.Find(r => (int)r["id"] == id);
        if (report == null) return Ok(false);
        report["lastRun"] = DateTime.Now;
        return Ok(report);
    }

    [HttpGet("search")]
    public IActionResult Search(string? q)
    {
        return Ok(_reports.FindAll(r => r["name"].ToString()!.Contains(q ?? "")));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var report = _reports.Find(r => (int)r["id"] == id);
        if (report == null) return Ok("missing");
        foreach (var key in payload.Keys) report[key] = payload[key];
        return Ok(report);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _reports.RemoveAll(r => (int)r["id"] == id);
        return Ok(true);
    }

    [HttpPost("bulk")]
    public IActionResult Bulk([FromBody] List<Dictionary<string, object>> payload)
    {
        foreach (var item in payload)
        {
            item["id"] = ++_counter;
            _reports.Add(item);
        }
        return Ok(payload.Count);
    }

    [HttpGet("export")]
    public IActionResult Export()
    {
        Thread.Sleep(5);
        return Ok(_reports.Select(r => r["id"]).ToList());
    }
}
