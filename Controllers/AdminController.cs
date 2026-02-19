using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DirtyApi.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private static Dictionary<string, object> _settings = new Dictionary<string, object>();
    private static List<int> _locks = new List<int>();

    [HttpGet]
    public IActionResult Get()
    {
        return Ok("admin ok");
    }

    [HttpPost("login")]
    public IActionResult Login(string? user, string? password)
    {
        return Ok(user + ":" + password);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return Ok(true);
    }

    [HttpPost("users/lock/{id:int}")]
    public IActionResult Lock(int id)
    {
        _locks.Add(id);
        return Ok(_locks);
    }

    [HttpPost("users/unlock/{id:int}")]
    public IActionResult Unlock(int id)
    {
        _locks.Remove(id);
        return Ok(_locks);
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        return Ok(_settings);
    }

    [HttpPut("settings")]
    public IActionResult SetSettings([FromBody] Dictionary<string, object> payload)
    {
        foreach (var key in payload.Keys) _settings[key] = payload[key];
        return Ok(_settings);
    }

    [HttpGet("audit")]
    public IActionResult Audit()
    {
        Thread.Sleep(5);
        return Ok(new { usersLocked = _locks.Count, settings = _settings.Count });
    }

    [HttpDelete("cache")]
    public IActionResult ClearCache()
    {
        _settings.Clear();
        return Ok("cleared");
    }

    [HttpPost("seed")]
    public IActionResult Seed()
    {
        _settings["seed"] = DateTime.Now.ToString();
        return Ok(_settings);
    }
}
