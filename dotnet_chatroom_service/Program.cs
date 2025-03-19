using Common.Helper.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.MyDbContext;
using Scrutor;
using Services.Implementation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("Authorization"); // 允许传递Authorization头
    });
});

builder.Services.Scan(scan => scan
     .FromApplicationDependencies(assembly =>
        assembly.FullName.StartsWith("Services") ||
        assembly.FullName.StartsWith("Common") ||
        assembly == typeof(Program).Assembly 
    )
    .AddClasses(classes => classes
        .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Helper"))
    )
    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

// db services 
builder.Services.AddDbContext<MyDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// jwt services
var jwtConfig = builder.Configuration.GetSection("JwtConfig");
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"])),
        ValidateIssuer = true,
        ValidIssuer = jwtConfig["Issuer"], 
        ValidateAudience = true,
        ValidAudience = jwtConfig["Audience"], 
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"認證失敗: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

// swagger services 
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNet Chatroom API", Version = "v1" });
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll"); // 启用 CORS

app.UseAuthentication(); // 必须先于 Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
