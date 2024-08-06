namespace HttpServerSim.App.Contracts;

public interface IRequestResponseLogger
{
    // id is needed to link requests/responses
    Task LogRequestAsync(HttpContext context, string id);
    Task LogResponseAsync(HttpContext context, string id);
}
