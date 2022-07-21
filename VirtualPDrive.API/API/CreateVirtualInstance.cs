using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using VirtualPDrive.API.Services.VC;

namespace VirtualPDrive.API.API;

/// <summary>
/// Virtual instace creation API controller.
/// </summary>
[Route("/api")]
[ApiController]
public partial class VirtualInstanceController : ControllerBase
{
    private readonly IVirtualClientManager _virtualClientManager;

    /// <summary>
    /// Creates a new instnace of the Destory controller.
    /// </summary>
    /// <param name="virtualClientManager">Client manager service.</param>
    public VirtualInstanceController(IVirtualClientManager virtualClientManager)
    {
        _virtualClientManager = virtualClientManager;
    }

    /// <summary>
    /// The create request data for the application.
    /// </summary>
    public class CreateRequest
    {
        /// <summary>
        /// Path to your ArmA 3 folder. Must have a value.
        /// </summary>
        [Required]
        public string Arma { get; set; } = "";
        /// <summary>
        /// A list of mods to whitelist. Leave blank to use all mods.
        /// </summary>
        public string[] Mods { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Set to true to skip loading mods.
        /// </summary>
        [DefaultValue(false)]
        public bool NoMods { get; set; } = false;
        /// <summary>
        /// The output directory for this instance.
        /// </summary>
        [DefaultValue("output")]
        public string Output { get; set; } = "output";
        /// <summary>
        /// Path to a local file structure to be loaded as well as ArmA 3 mods.
        /// </summary>
        [DefaultValue(null)]
        public string? Local { get; set; } = null;
    }

    /// <summary>
    /// Response to the Create Instance call.
    /// </summary>
    public class CreateResponse
    {
        /// <summary>
        /// The ID of the newly created instance.
        /// </summary>
        [Required]
        public string InstanceId { get; set; }
    }

    /// <summary>
    /// Creates a new instance of a Virtual P Drive.
    /// </summary>
    /// <param name="args">Arguments to build the drive with.</param>
    /// <returns>An <see cref="IActionResult"/> for this request.</returns>
    /// <response code="202">Returns the ID of the virtual instace that was attempted to be created.</response>
    [HttpPost("create", Name = "CreateInstance")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(CreateResponse))]
    [Produces("application/json")]
    public IActionResult CreateInstance(CreateRequest args)
    {
        var container = _virtualClientManager.CreateVirtualClient(new()
        {
            ArmAPath = args.Arma,
            ModsFilter = args.Mods,
            NoMods = args.NoMods,
            OutputPath = args.Output,
            Local = args.Local
        });

        return Accepted($"/api/instance/{container.Id}", 
            new CreateResponse()
            {
                InstanceId = container.Id,
            });
    }
}
