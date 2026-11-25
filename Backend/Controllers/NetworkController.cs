
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/network")]
public sealed class NetworkController : ControllerBase
{
    private readonly MeshNetworkService _network;

    public NetworkController(MeshNetworkService network)
    {
        _network = network;
    }

    public record LinkRequest(
        int SourceId,
        int TargetId,
        double Cost
    );

    // View the graph's nodes and connections.
    [HttpGet("topology")]
    public IActionResult Topology()
    {
        return Ok(_network.GetTopology());
    }

    // Create a weighted network connection.
    [HttpPost("links")]
    public IActionResult AddLink(
        [FromBody] LinkRequest request)
    {
        try
        {
            _network.AddLink(
                request.SourceId,
                request.TargetId,
                request.Cost);

            return Ok(new
            {
                message = "Network connection created.",
                request.SourceId,
                request.TargetId,
                request.Cost
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Delete a network connection.
    [HttpDelete("links/{sourceId:int}/{targetId:int}")]
    public IActionResult RemoveLink(
        int sourceId,
        int targetId)
    {
        bool removed = _network.RemoveLink(
            sourceId, targetId);

        return removed
            ? Ok(new { message = "Connection removed." })
            : NotFound(new
            {
                message = "Connection does not exist."
            });
    }

    // Explore connected devices with BFS or DFS.
    [HttpGet("traverse")]
    public IActionResult Traverse(
        [FromQuery] int startSensorId,
        [FromQuery] string algorithm = "bfs")
    {
        try
        {
            return Ok(_network.Traverse(
                startSensorId, algorithm));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Find the lowest-total-cost spanning network.
    [HttpGet("mst")]
    public IActionResult MinimumSpanningTree()
    {
        return Ok(_network.CalculateMst());
    }
}
