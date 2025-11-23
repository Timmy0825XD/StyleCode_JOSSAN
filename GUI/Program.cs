using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using BLL.Implementaciones;
using BLL.Interfaces;
using CloudinaryDotNet;
using DAL.Implementaciones;
using DAL.Interfaces;
using GUI.Components;
using GUI.Services;
using Oracle.ManagedDataAccess.Client;
using static GUI.Services.VirtualTryOnApp;

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


// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("OracleConnection")!;

// ===== REGISTRAR SERVICIOS DE LA CAPA DAL =====
builder.Services.AddScoped<IUsuarioDAO>(sp => new UsuarioDAO(connectionString));
builder.Services.AddScoped<IRolDAO>(sp => new RolDAO(connectionString));
builder.Services.AddScoped<IArticuloDAO>(sp => new ArticuloDAO(connectionString));
builder.Services.AddScoped<ICategoriaDAO>(sp => new CategoriaDAO(connectionString));
builder.Services.AddScoped<IDireccionDAO>(sp => new DireccionDAO(connectionString));
builder.Services.AddScoped<IPedidoDAO>(sp => new PedidoDAO(connectionString));
builder.Services.AddScoped<IEmailDAO>(sp => new EmailDAO(connectionString));
builder.Services.AddScoped<IFavoritoDAO>(sp => new FavoritoDAO(connectionString));
builder.Services.AddScoped<IEstadisticaDAO>(sp => new EstadisticaDAO(connectionString));
builder.Services.AddScoped<IAlertaDAO>(sp => new AlertaDAO(connectionString));
builder.Services.AddScoped<ICuponDAO>(sp => new CuponDAO(connectionString));
builder.Services.AddScoped<IReporteDAO>(sp => new ReporteDAO(connectionString));

// ===== REGISTRAR SERVICIOS DE LA CAPA BLL =====
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IArticuloServices, ArticuloServices>();
builder.Services.AddScoped<ICategoriaServices, CategoriaServices>();
builder.Services.AddScoped<IDireccionService, DireccionService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();
builder.Services.AddScoped<IEstadisticaService, EstadisticaService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<ICuponService, CuponService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// ===== SERVICIOS AUXILIARES =====
builder.Services.AddScoped<SesionService>();
builder.Services.AddScoped<CarritoService>();
builder.Services.AddHostedService<EmailBackgroundService>();

// Registrar DAOs y Services de Facturación
builder.Services.AddScoped<IFacturaDAO>(provider =>
    new FacturaDAO(builder.Configuration.GetConnectionString("OracleConnection")));
builder.Services.AddScoped<IFacturaService, FacturaService>();

// ===== VIRTUAL TRY-ON SERVICE =====
// IMPORTANTE: Registrar HttpClient con VirtualTryOnService
builder.Services.AddHttpClient<VirtualTryOnService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000");
    client.Timeout = TimeSpan.FromMinutes(5); // Try-on puede tardar
});

// ===== CLOUDINARY CONFIGURATION =====
var cloudinaryUrl = builder.Configuration["Cloudinary:URL"]!;
var uri = new Uri(cloudinaryUrl.Replace("cloudinary://", "http://"));
var userInfo = uri.UserInfo.Split(':');
var cloudName = uri.Host;
var apiKey = userInfo[0];
var apiSecret = userInfo[1];
var account = new Account(cloudName, apiKey, apiSecret);
builder.Services.AddSingleton(new Cloudinary(account));

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