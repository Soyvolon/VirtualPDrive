using Microsoft.AspNetCore.Mvc;

using VirtualPDrive.API.Services.VC;

namespace VirtualPDrive.API.API;

public partial class VirtualInstanceController : ControllerBase
{
    /// <summary>
    /// The response class for the destroy requests.
    /// </summary>
    public class DestroyInstanceResponse
    {
        /// <summary>
        /// The ID of the instance that was requested to be destroyed. Is null
        /// of no instance is found.
        /// </summary>
        public string? InstanceId { get; set; }
        /// <summary>
        /// True if the instance was destroyed successfuly.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Messages from the client.
        /// </summary>
        public string[] Messages { get; set; }
    }

    /// <summary>
    /// Handles requests for the destruction of virtual instnaces.
    /// </summary>
    /// <param name="id">The id of the instance to destroy.</param>
    /// <returns>An <see cref="IActionResult"/> for this action.</returns>
    /// <status code="400">The provided ID was not found.</status>
    /// <status code="200">The instance was destroyed successfuly.</status>
    [HttpDelete("destroy/{id}", Name = "DestroyInstance")]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DestroyInstanceResponse))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DestroyInstanceResponse))]
    [Produces("application/json")]
    public IActionResult DestoryVirtualDriveRequestAsync(string id)
    {
        var container = _virtualClientManager.DestroyVirtualClient(id);

        if (container is null)
        {
            return BadRequest(new DestroyInstanceResponse()
            {
                InstanceId = null,
                Success = false,
                Messages = new string[] { $"No instance by the ID of {id} was found" }
            });
        }

        return Ok(new DestroyInstanceResponse()
        {
            InstanceId = container.Id,
            Success = true,
            Messages = new string[] { "Instance destoryed." }
        });
    }
}
