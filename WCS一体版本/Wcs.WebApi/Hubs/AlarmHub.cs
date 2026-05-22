using Microsoft.AspNetCore.SignalR;

namespace Wcs.WebApi.Hubs;

public sealed class AlarmHub : Hub
{
    public Task<string> Ping()
    {
        return Task.FromResult("alarm-hub-ok");
    }
}
