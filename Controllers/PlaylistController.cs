using System;
using Microsoft.AspNetCore.Mvc;
using MongoExample.Services;
using MongoExample.Models;
using MongoDB.Bson;

namespace MongoExample.Controllers;

[Controller]
[Route("api/[controller]")]
public class PlaylistController : Controller
{

    private readonly MongoDBService _mongoDBService;
    private readonly ILogger _logger;

    public PlaylistController(MongoDBService mongoDBService, ILogger<PlaylistController> logger)
    {
        _mongoDBService = mongoDBService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<List<Playlist>> Get()
    {
        return await _mongoDBService.GetAsync();
    }

     [HttpGet("list")]
    public async Task<List<BsonDocument>> GetList()
    {
        return await _mongoDBService.GetLists();
        // return NoContent();
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Playlist playlist)
    {
        await _mongoDBService.CreateAsync(playlist);
        return CreatedAtAction(nameof(Get), new { id = playlist.Id }, playlist);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AddToPlaylist(string id, [FromBody] string movieId)
    {
        await _mongoDBService.AddToPlaylistAsync(id, movieId);
        return NoContent();
    }

    [HttpPut("updatePlaylist")]
    public async Task<ActionResult<Playlist>> UpdatePlaylist(string id, [FromBody] Playlist playlist)
    {
        await _mongoDBService.UpdateAsync(id, playlist);
        return await _mongoDBService.FindOne(id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mongoDBService.DeleteAsync(id);
        return NoContent();
    }

}