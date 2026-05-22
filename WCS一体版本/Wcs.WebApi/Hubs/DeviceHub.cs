using Microsoft.AspNetCore.SignalR;

namespace Wcs.WebApi.Hubs;

public sealed class DeviceHub : Hub
{
    public Task<string> Ping()
    {
        return Task.FromResult("device-hub-ok");
    }
}
