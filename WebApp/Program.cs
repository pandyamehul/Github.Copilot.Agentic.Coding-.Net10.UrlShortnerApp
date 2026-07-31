using UrlTrimmer.WebApp;
using UrlTrimmer.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<UrlShortenerApiClient>(client =>
{
    var baseUrl = builder.Configuration["UrlShortenerApi:BaseUrl"] ?? "http://localhost:5043/";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();