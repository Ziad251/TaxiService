using mapsmvcwebapp.Models;
using mapsmvcwebapp.Services;
using mapsmvcwebapp.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<NumberToString>();
builder.Services.AddHttpClient<FakeUsers>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)) //Set lifetime to five minutes
        .AddPolicyHandler(GetRetryPolicy());

        
        // Add user database from mongoDB
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));
builder.Services.AddSingleton<NumberToString>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IGetClaimsProvider, GetClaimsFromUser>();
builder.Services.AddSingleton<mongoServiceUser>();
builder.Services.AddSingleton<IFakeUsers, FakeUsers>();




builder.Services.AddAuthentication(CookieAuthenticationDefaults
            .AuthenticationScheme)
            .AddCookie(option =>
            {
                option.LoginPath = "/Login";
            });

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

var app = builder.Build();

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireAuthorization();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}