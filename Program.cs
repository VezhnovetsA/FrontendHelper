using FHDatabase;
using FHDatabase.Repositories;
using FrontendHelper.Services;
using FrontendHelper.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(AuthService.AUTH_TYPE)
    .AddCookie(AuthService.AUTH_TYPE, config =>
    {
        config.LoginPath = "/Authentication/Login";
        config.AccessDeniedPath = "/Authentication/AccessDeniedPage";
    });


builder.Services.AddDbContext<FhDbContext>(options =>
    options.UseSqlServer(FhDbContext.CONNECTION_STRING));

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IconRepository>();
builder.Services.AddScoped<PictureRepository>();
builder.Services.AddScoped<AnimatedElementRepository>();
builder.Services.AddScoped<ButtonRepository>();
builder.Services.AddScoped<FontRepository>();
builder.Services.AddScoped<FormRepository>();
builder.Services.AddScoped<ColorRepository>();
builder.Services.AddScoped<PaletteRepository>();
builder.Services.AddScoped<TemplateRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<FilterRepository>();
builder.Services.AddScoped<FavoriteRepository>();
builder.Services.AddScoped<UserAssetRepository>();


builder.Services.AddSingleton<QrCodeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();




var app = builder.Build();

FHDatabase.Seed.CheckAndFillWithDefaultEntytiesDatabase(app.Services);


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
