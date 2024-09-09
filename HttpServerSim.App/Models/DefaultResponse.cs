using HttpServerSim.Client.Models;

namespace HttpServerSim.App.Models;

internal class DefaultResponse
{
    public required HttpSimResponse Response { get; set; }
    public DelayRange? Delay { get; set; }
}
