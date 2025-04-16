using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Repositories.HttpClients;
using Repositories.MyDbContext;
using Scrutor;
using Services.Publish;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// 註冊 HttpClient 服務
builder.Services.AddHttpClient<IChatServiceApiClient, ChatServiceApiClient>(client => { })
    .SetHandlerLifetime(TimeSpan.FromMinutes(15));

builder.Services.AddHttpClient<IChatServiceStreamClient, ChatServiceStreamClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ChatServiceApiSettings:BaseUrl"]);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("text/event-stream"));
});

// 加入重試策略
builder.Services.AddHttpClient<IChatServiceApiClient, ChatServiceApiClient>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

builder.Services.AddScoped<ITokenProvider, HttpContextTokenProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(
                "https://oniind244.online",
                "http://localhost:5173",
                "http://localhost:11116")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// services
builder.Services.Scan(scan => scan
     .FromApplicationDependencies(assembly =>
        assembly.FullName.StartsWith("Services") ||
        assembly.FullName.StartsWith("Common") ||
        assembly == typeof(Program).Assembly
    )
    .AddClasses(classes => classes
        .Where(t =>
            (t.Name.EndsWith("Service") || t.Name.EndsWith("Helper")) &&
            !typeof(IHostedService).IsAssignableFrom(t) // 排除实现了 IHostedService 的类
        )
    )
    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

// publish service
builder.Services.AddHostedService<OutboxPublisherService>();

// db services 
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// db repository 
builder.Services.AddScoped(typeof(IRepository<>), typeof(MyRepository<>));

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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"jwt auth error : {context.Exception.Message}");
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

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("MyAllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();
app.Use((context, next) =>
{
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=()");
    return next();
});
app.MapControllers();

app.Run();