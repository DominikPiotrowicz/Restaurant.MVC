using Infrastructure.Extensions;
using Infrastructure.Seeders;
using Application.Extensions;
using Restaurant.MVC.Middleware;
using Serilog;
using Serilog.Events;
using AspNetCoreRateLimit;

// Konfiguracja Serilog
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
	.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.Enrich.WithThreadId()
	.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
	.WriteTo.File(
		path: "logs/restaurant-.log",
		rollingInterval: RollingInterval.Day,
		outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
		retainedFileCountLimit: 30)
	.WriteTo.File(
		path: "logs/errors/restaurant-errors-.log",
		rollingInterval: RollingInterval.Day,
		restrictedToMinimumLevel: LogEventLevel.Error,
		retainedFileCountLimit: 90)
	.CreateLogger();

try
{
	Log.Information("Starting Restaurant.MVC application");

	var builder = WebApplication.CreateBuilder(args);

	// Użyj Serilog jako głównego loggera
	builder.Host.UseSerilog();

	// Add services to the container.
	builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

	// Rate Limiting
	builder.Services.AddMemoryCache();
	builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
	builder.Services.AddInMemoryRateLimiting();
	builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

	builder.Services.AddHttpContextAccessor();

	builder.Services.AddInfrastructure(builder.Configuration);
	builder.Services.AddApplication();

	var app = builder.Build();

	var scope = app.Services.CreateScope();
	var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();
	var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();

	await roleSeeder.Seed();
	await seeder.Seed();

	// Middleware - kolejność jest ważna!

	// 1. Serilog Request Logging
	app.UseSerilogRequestLogging();

	// 2. Security Headers
	app.UseMiddleware<SecurityHeadersMiddleware>();

	// 3. Exception Handling
	app.UseMiddleware<ExceptionHandlingMiddleware>();

	// 4. Request Logging
	app.UseMiddleware<RequestLoggingMiddleware>();

	// 5. Rate Limiting
	app.UseIpRateLimiting();

	if (!app.Environment.IsDevelopment())
	{
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

	Log.Information("Restaurant.MVC application started successfully");

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}
