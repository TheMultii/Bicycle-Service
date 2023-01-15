using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serwer;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Reflection;
using System.Text;

const string baseAPIVersion = "v1";
const string baseAPIName = "Serwis Rowerowy";

string addZero(int x) => x < 10 ? $"0{x}" : x.ToString();

string getAPIVersion() {
    DateTime dateTime = DateTime.Now;
    string monthDate = $"{addZero(dateTime.Month)}{addZero(dateTime.Day)}";
    return $"{baseAPIVersion}-{monthDate}";
}

ENVLoader.Load(".env");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options => {
    options.CustomizeProblemDetails = (context) => {
        context.ProblemDetails.Title = "An error occurred!";
        context.ProblemDetails.Status = (int)HttpStatusCode.InternalServerError;
        context.ProblemDetails.Detail = context.ProblemDetails.Detail;
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Type = null;
    };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
        Description = "Standard Authorization header using the Bearer scheme (\"Bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.SwaggerDoc(baseAPIVersion, new OpenApiInfo {
        Version = getAPIVersion(),
        Title = baseAPIName,
        Description = "An ASP.NET Core Web API for managing bicycle service",
        License = new OpenApiLicense {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    

    var fileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
    options.IncludeXmlComments(filePath);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                ENVLoader.GetString("TOKEN")
            )),
            ValidateIssuer = false,
            ValidateAudience = false
        };
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

app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// db init
if (!Directory.Exists("database")) {
    Directory.CreateDirectory("database");
}
SqliteConnection _connection = new("Data Source=database/serwis.sqlite");
_connection.Open();
string script = File.ReadAllText("init_migration_tables.sql");
Console.WriteLine(script);
var command = _connection.CreateCommand();
command.CommandText = script;
command.ExecuteNonQuery();
_connection.Close();

app.Run();
