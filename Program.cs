using FrontendHelper.Data;
using FrontendHelper.interfaces;
using FrontendHelper.mocks;
using FrontendHelper.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// подключение к бд
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// службы мвс
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAllIcons, IconRepository>();
builder.Services.AddScoped<IAllPictures, PictureRepository>();
builder.Services.AddScoped<IAllAnimatedElements, AnimatedElementRepository>();
builder.Services.AddScoped<IAllButtons, ButtonRepository>();
builder.Services.AddScoped<IAllFonts, FontRepository>();


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// рейзор для интерфейса авторизации
builder.Services.AddRazorPages();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
}


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
