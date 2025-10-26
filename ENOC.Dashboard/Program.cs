using ENOC.Dashboard.Components;
using ENOC.Dashboard.Configuration;
using ENOC.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("ApiConfig"));

// HttpClient for API calls
builder.Services.AddHttpClient<ApiService>();

// API Services
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<IncidentApiService>();
builder.Services.AddScoped<LookupApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<TankApiService>();
builder.Services.AddScoped<WeatherApiService>();
builder.Services.AddScoped<MapApiService>();
builder.Services.AddScoped<AlertApiService>();

// State Management
builder.Services.AddScoped<AuthStateService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
