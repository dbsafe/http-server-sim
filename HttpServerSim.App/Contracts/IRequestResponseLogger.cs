namespace HttpServerSim.App.Contracts;

public interface IRequestResponseLogger
{
    Task LogRequestAsync(HttpContext context);
    Task LogResponseAsync(HttpContext context);
}
