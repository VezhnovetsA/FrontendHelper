using FHDatabase;
using FHDatabase.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

////подключение к бд
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<FhDbContext>(options =>
    options.UseSqlServer(FhDbContext.CONNECTION_STRING));

// службы мвс
builder.Services.AddControllersWithViews();

// регистрация репозиториев
builder.Services.AddScoped<IconRepository>();
builder.Services.AddScoped<PictureRepository>();
builder.Services.AddScoped<AnimatedElementRepository>();
builder.Services.AddScoped<ButtonRepository>();
builder.Services.AddScoped<FontRepository>();
builder.Services.AddScoped<FormRepository>();

//регистрация сервисов
builder.Services.AddSingleton<QrCodeService>();

//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();


// рейзор для интерфейса авторизации
builder.Services.AddRazorPages();

var app = builder.Build();


//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<ApplicationDbContext>();
//    DbInitializer.Initialize(context);
//}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


//void ConfigureServices(IServiceCollection services)
//{
//    services.AddTransient<IAllIcons, MockIcons>();
//    services.AddTransient<IAllPictures, MockPictures>();
//    services.AddMvc();
//}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
