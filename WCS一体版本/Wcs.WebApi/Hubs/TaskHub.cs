using Microsoft.AspNetCore.SignalR;

namespace Wcs.WebApi.Hubs;

public sealed class TaskHub : Hub
{
    public Task<string> Ping()
    {
        return Task.FromResult("task-hub-ok");
    }
}
