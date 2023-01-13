using Microsoft.OpenApi.Models;
const string baseAPIVersion = "v1";
const string baseAPIName = "Serwis Rowerowy";

string addZero(int x) => x < 10 ? $"0{x}" : x.ToString();

string getAPIVersion() {
    DateTime dateTime = DateTime.Now;
    string monthDate = $"{addZero(dateTime.Month)}{addZero(dateTime.Day)}";
    return $"{baseAPIVersion}-{monthDate}";
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc(baseAPIVersion, new OpenApiInfo {
        Version = getAPIVersion(),
        Title = baseAPIName,
        Description = "An ASP.NET Core Web API for managing bicycle service",
        License = new OpenApiLicense {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();

    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", baseAPIName);
        c.RoutePrefix = "docs";
        c.DocumentTitle = $"{baseAPIName} | Docs";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
