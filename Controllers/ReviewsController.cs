using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/reviews")]
public class ReviewsController : ControllerBase
{
    private static List<Dictionary<string, object>> _reviews = new List<Dictionary<string, object>>();
    private static int _counter = 0;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_reviews);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok(_reviews.Find(r => (int)r["id"] == id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        _reviews.Add(payload);
        return Ok(payload);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var review = _reviews.Find(r => (int)r["id"] == id);
        if (review == null) return Ok("missing");
        foreach (var key in payload.Keys) review[key] = payload[key];
        return Ok(review);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _reviews.RemoveAll(r => (int)r["id"] == id);
        return Ok(true);
    }

    [HttpGet("{id:int}/approve")]
    public IActionResult Approve(int id)
    {
        var review = _reviews.Find(r => (int)r["id"] == id);
        if (review == null) return Ok(false);
        review["status"] = "approved";
        return Ok(review);
    }

    [HttpPost("{id:int}/reject")]
    public IActionResult Reject(int id, string? reason)
    {
        var review = _reviews.Find(r => (int)r["id"] == id);
        if (review == null) return Ok(false);
        review["status"] = "rejected";
        review["reason"] = reason ?? "none";
        return Ok(review);
    }

    [HttpGet("search")]
    public IActionResult Search(string? q)
    {
        return Ok(_reviews.FindAll(r => r["text"].ToString()!.Contains(q ?? "")));
    }

    [HttpPost("bulk")]
    public IActionResult Bulk([FromBody] List<Dictionary<string, object>> payload)
    {
        foreach (var item in payload)
        {
            item["id"] = ++_counter;
            _reviews.Add(item);
        }
        return Ok(payload.Count);
    }

    [HttpGet("stats")]
    public IActionResult Stats()
    {
        Thread.Sleep(5);
        return Ok(new { count = _reviews.Count, last = _reviews.LastOrDefault() });
    }
}
