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
        /// <summary>
        /// An array of file extensions that are allowed to be initalized for file loading.
        /// </summary>
        public string[] Extensions { get; set; } = Array.Empty<string>();
        /// <summary>
        /// A whitelist of file names that are allowed to be initalized for file loading.
        /// </summary>
        public string[] Whitelist { get; set; } = Array.Empty<string>();
        /// <summary>
        /// If ture, the file system will preload all allowed files/extensions before starting.
        /// </summary>
        [DefaultValue(false)]
        public bool PreLoad { get; set; } = false;
        /// <summary>
        /// The ammount of concurent initalize operations that can occour at once.
        /// </summary>
        [DefaultValue(2)]
        public int InitRunners { get; set; } = 2;
        /// <summary>
        /// If true, the file system wont clean the output folder before starting.
        /// </summary>
        [DefaultValue(false)]
        public bool NoClean { get; set; } = false;
        /// <summary>
        /// Generate a random output folder if the requested one is not avalible.
        /// </summary>
        [DefaultValue(true)]
        public bool RandomOutput { get; set; } = false;
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
        /// <summary>
        /// The path where the API created a virtual instance.
        /// </summary>
        [Required]
        public string Path { get; set; }
    }

    /// <summary>
    /// Response when a bad request to create an instance was sent.
    /// </summary>
    public class InvlaidCreateRequest
    {
        /// <summary>
        /// The messages returned from the creation request.
        /// </summary>
        [Required]
        public string[] Messages { get; set; }
    }

    /// <summary>
    /// Creates a new instance of a Virtual P Drive.
    /// </summary>
    /// <param name="args">Arguments to build the drive with.</param>
    /// <returns>An <see cref="IActionResult"/> for this request.</returns>
    /// <response code="202">Returns the ID of the virtual instace that was attempted to be created.</response>
    [HttpPost("create", Name = "CreateInstance")]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(CreateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(InvlaidCreateRequest))]
    [Produces("application/json")]
    public IActionResult CreateInstance(CreateRequest args)
    {
        try
        {
            var container = _virtualClientManager.CreateVirtualClient(new()
            {
                ArmAPath = args.Arma,
                ModsFilter = args.Mods,
                NoMods = args.NoMods,
                OutputPath = args.Output,
                Local = args.Local,
                InitRunners = args.InitRunners,
                NoClean = args.NoClean,
                PreLoad = args.PreLoad,
                PreloadWhitelist = args.Whitelist,
                ReadableExtensions = args.Extensions
            }, args.RandomOutput, string.IsNullOrWhiteSpace(args.Output));

            if (container.Errored)
            {
                return BadRequest(new InvlaidCreateRequest()
                {
                    Messages = container.MessageStack.ToArray()
                });
            }

            return Accepted($"/api/instance/{container.Id}",
                new CreateResponse()
                {
                    InstanceId = container.Id,
                    Path = container.Client.Settings.OutputPath,
                });
        }
        catch (Exception ex)
        {
            return BadRequest(new InvlaidCreateRequest()
            {
                Messages = new string[] { ex.Message, "Failed to create a new instance." }
            });
        }
    }
}
