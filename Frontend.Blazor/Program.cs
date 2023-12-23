using System.Reflection;
using Frontend.Blazor.Code;
using Frontend.Blazor.HttpClients;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<LabService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddHttpClient<IBackendApiHttpClient, BackendApiHttpClient>(options =>
{
    options.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Urls:BackendApi"));
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DefaultRequestHeaders.TryAddWithoutValidation("Service",
        Assembly.GetAssembly(typeof(Program))?.GetName().Name);
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 209715200; // 200 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();