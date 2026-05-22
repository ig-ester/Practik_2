using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Praktik.Application.Interfaces;
using Praktik.Application.Services;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;
using Praktik.Infrastructure.Stubs;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.LogoutPath = "/Account/Logout";
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opt.AddPolicy("ManagerOrAdmin", p => p.RequireRole("Manager", "Admin"));
    opt.AddPolicy("CashierOrManager", p => p.RequireRole("Cashier", "Manager", "Admin"));
});

builder.Services.AddScoped<IMarkingService, MarkingStub>();
builder.Services.AddScoped<IFiscalService, FiscalStub>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<DocumentNumberService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SalesService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

using var scope = app.Services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
if (ctx.Database.GetPendingMigrations().Any()) ctx.Database.Migrate();

if (!ctx.Users.Any())
{
    var roles = new List<Role>
    {
        new Role { Name = "Admin", Description = "Полный доступ ко всем функциям системы" },
        new Role { Name = "Manager", Description = "Управление документами и справочниками" },
        new Role { Name = "Cashier", Description = "Розничные продажи" }
    };
    ctx.Roles.AddRange(roles);
    ctx.SaveChanges();

    var org = new Organization
    {
        Name = "Аптека №1",
        Address = "город, улица, дом",
        CreatedAt = DateTime.UtcNow
    };
    ctx.Organizations.Add(org);
    ctx.SaveChanges();

    var users = new List<User>
    {
        new User
        {
            Login = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            RoleId = roles[0].Id,
            OrganizationId = org.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new User
        {
            Login = "manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            RoleId = roles[1].Id,
            OrganizationId = org.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new User
        {
            Login = "cashier",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            RoleId = roles[2].Id,
            OrganizationId = org.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };
    ctx.Users.AddRange(users);
    ctx.SaveChanges();
}
app.Run();
