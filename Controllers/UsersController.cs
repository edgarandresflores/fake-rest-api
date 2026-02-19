using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private static List<Dictionary<string, object>> _users = new List<Dictionary<string, object>>();
    private static int _counter = 0;
    private static Random _rnd = new Random();

    [HttpGet]
    public IActionResult GetAll()
    {
        if (_users.Count == 0)
        {
            return Ok("no users");
        }
        return Ok(_users);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var user = _users.Find(u => (int)u["id"] == id);
        return Ok(user == null ? new { error = "missing" } : user);
    }

    [HttpGet("search")]
    public IActionResult Search(string? q)
    {
        var result = _users.FindAll(u => u["name"].ToString()!.Contains(q ?? ""));
        return Ok(result);
    }

    [HttpGet("create")]
    public IActionResult CreateViaGet(string? name, int age = 0)
    {
        _counter++;
        var user = new Dictionary<string, object>
        {
            ["id"] = _counter,
            ["name"] = name ?? "unknown",
            ["age"] = age,
            ["created"] = DateTime.Now
        };
        _users.Add(user);
        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Dictionary<string, object> payload)
    {
        payload["id"] = ++_counter;
        payload["created"] = DateTime.Now;
        _users.Add(payload);
        return Ok(payload);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Dictionary<string, object> payload)
    {
        var user = _users.Find(u => (int)u["id"] == id);
        if (user == null) return Ok("not found");
        foreach (var key in payload.Keys) user[key] = payload[key];
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _users.RemoveAll(u => (int)u["id"] == id);
        return Ok("deleted");
    }

    [HttpPost("import")]
    public IActionResult Import(string? source)
    {
        Thread.Sleep(10);
        _users.Add(new Dictionary<string, object> { ["id"] = ++_counter, ["name"] = source ?? "imported" });
        return Ok(true);
    }

    [HttpGet("export")]
    public IActionResult Export()
    {
        return Ok(string.Join(";", _users.Select(u => u["id"] + ":" + u["name"])));
    }

    [HttpPost("{id:int}/toggle")]
    public IActionResult Toggle(int id)
    {
        var user = _users.Find(u => (int)u["id"] == id);
        if (user == null) return Ok(false);
        user["active"] = !(user.ContainsKey("active") && (bool)user["active"]);
        return Ok(user);
    }
}
