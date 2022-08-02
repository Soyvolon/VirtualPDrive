using PDriveUtility.Structures.Settings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Settings;
public interface ISettingsService
{
    // Setting objects.
    public LoggingSettings LoggingSettings { get; protected set; }
    public ApplicationSettings ApplicationSettings { get; protected set; }
    public StartupFlags StartupFlags { get; protected set; }

    public Task SaveApplicationSettingsAsync();

    public Task InitalizeAsync();
    public void ReloadLogger();
}
