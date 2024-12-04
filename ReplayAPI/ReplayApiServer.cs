namespace ReplayAPI;

public class ReplayApiServer
{
    private bool _running;
    public void Run()
    {
        if (_running)
        {
            return;
        }
        
        var builder = WebApplication.CreateBuilder([]);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        //app.UseAuthorization();


        app.MapControllers();

        app.Run();

        _running = true;
    }
}