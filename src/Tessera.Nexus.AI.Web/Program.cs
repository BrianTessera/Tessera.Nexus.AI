using Tessera.Nexus.AI.Application.Contracts;
using Tessera.Nexus.AI.Infrastructure.Configuration;
using Tessera.Nexus.AI.Infrastructure.Database;
using Tessera.Nexus.AI.Infrastructure.DependencyInjection;
using Tessera.Nexus.AI.Infrastructure.Repositories;
using Tessera.Nexus.AI.Web.Components;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddInfrastructure();
builder.Services.AddScoped<IDatabaseHealthCheckService,
                   DatabaseHealthCheckService>();

builder.Services.AddScoped<IApplicationSettingRepository,
                   ApplicationSettingRepository>();

builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection(
        OllamaSettings.SectionName));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
