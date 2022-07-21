using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;

using VirtualPDrive.API.Services.VC;

namespace VirtualPDrive.API.API;

public partial class VirtualInstanceController : ControllerBase
{
    /// <summary>
    /// The result data for a staus request.
    /// </summary>
    public class GetInstanceResult
    {
        /// <summary>
        /// The ID of the instance that was requested to be destroyed. Is null
        /// of no instance is found.
        /// </summary>
        public string? InstanceId { get; set; }
        /// <summary>
        /// True if the instance is fully loaded.
        /// </summary>
        public bool Loaded { get; set; }
        /// <summary>
        /// True if the instance has permanetly stopped.
        /// </summary>
        public bool Stopped { get; set; }
        /// <summary>
        /// True if the instance has errored.
        /// </summary>
        public bool Errored { get; set; }
        /// <summary>
        /// An array of messages from the instance.
        /// </summary>
        public string[] Messages { get; set; }
    }

    /// <summary>
    /// Handles requests for the stauts of virtual instnaces.
    /// </summary>
    /// <param name="id">The id of the instance to get status about.</param>
    /// <returns>An <see cref="IActionResult"/> for this action.</returns>
    /// <status code="400">The provided ID was not found.</status>
    /// <status code="200">The instance was retrived successfuly.</status>
    [HttpGet("instance/{id}", Name = "GetInstance")]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(GetInstanceResult))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetInstanceResult))]
    [Produces("application/json")]
    public IActionResult GetInstance(string id)
    {
        var container = _virtualClientManager.GetVirtualClient(id);

        if (container is null)
        {
            return BadRequest(new GetInstanceResult()
            {
                InstanceId = null,
                Loaded = false,
                Errored = true,
                Stopped = true,
                Messages = new string[] { $"No instance by the ID of {id} was found" }
            });
        }

        return Ok(new GetInstanceResult()
        {
            InstanceId = id,
            Loaded = container.Loaded,
            Errored = container.Errored,
            Stopped = container.Shutdown,
            Messages = container.MessageStack.ToArray()
        });
    }
}
