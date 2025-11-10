using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using BLL.Implementaciones;
using BLL.Interfaces;
using DAL.Implementaciones;
using DAL.Interfaces;
using GUI.Components;
using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar Wallet PRIMERO, antes de cualquier conexión
var walletLocation = builder.Configuration["OracleWallet:Location"]!;
OracleConfiguration.TnsAdmin = walletLocation;
OracleConfiguration.WalletLocation = walletLocation;

// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("OracleConnection")!;

// ===== REGISTRAR SERVICIOS DE LA CAPA DAL =====
builder.Services.AddScoped<IUsuarioDAO>(sp => new UsuarioDAO(connectionString));
builder.Services.AddScoped<IRolDAO>(sp => new RolDAO(connectionString));
builder.Services.AddScoped<IArticuloDAO>(sp => new ArticuloDAO(connectionString));
builder.Services.AddScoped<ICategoriaDAO>(sp => new CategoriaDAO(connectionString));

// ===== REGISTRAR SERVICIOS DE LA CAPA BLL =====
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IArticuloServices, ArticuloServices>();
builder.Services.AddScoped<ICategoriaServices, CategoriaServices>();

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
