using AuthenticationTokenServiceAPI;
using AuthenticationTokenServiceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

#if ApiConventions
[assembly: ApiConventionType(typeof(DefaultApiConventions))]
#endif

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.UseStaticApplicationConfiguration();

builder.Services.AddMvc(options =>
{
    // assume that we want our api all prefix route with APIBaseInfo.BasePath=DEV_API_V1 for development phrase.
    // we also could switch appsettings to Qas/Release that prefix route with different path:
    // QAS_API_V1/RELEASE_API_V1 
    options.UseCentralRoutePrefix(new RouteAttribute($"{ApplicationConfiguration.ApplicationSettings.BasePath}/"));
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(ApplicationConfiguration.SwaggerSettings.Version, new OpenApiInfo
    {
        Title = ApplicationConfiguration.SwaggerSettings.APIName,
        Version = ApplicationConfiguration.SwaggerSettings.Version
    });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[]{}
        }
    });


});


builder.Services.AddScoped<UserManager>();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = JwtTokenConfig.Default.ToTokenValidationParameters();
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (ApplicationConfiguration.SwaggerSettings.UseDeveloperExceptionPage)
{
    app.UseDeveloperExceptionPage();
}
string swaggerRoutePrefix = $"{ApplicationConfiguration.ApplicationSettings.BasePath}/{ApplicationConfiguration.SwaggerSettings.BasePath}";
app.UseSwagger(c => c.RouteTemplate = $"{swaggerRoutePrefix}/{{documentName}}/{ApplicationConfiguration.SwaggerSettings.APIName}.json");
if (ApplicationConfiguration.SwaggerSettings.UseSwaggerUI)
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"{ApplicationConfiguration.SwaggerSettings.Version}/{ApplicationConfiguration.SwaggerSettings.APIName}.json", $"{ApplicationConfiguration.SwaggerSettings.APIName} - {ApplicationConfiguration.SwaggerSettings.Version}");
        c.RoutePrefix = swaggerRoutePrefix;
    });
}
app.UsePathBase(new PathString($"/{ApplicationConfiguration.ApplicationSettings.BasePath}"));

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.Run();
