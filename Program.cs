using FunerariaAPI.Data; // Asegúrate de que este sea el namespace correcto de tu DbContext
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1. BASE DE DATOS (CONFIGURADO PARA SUPABASE / POSTGRESQL)
// -----------------------------------------------------------------------------
// Nota: Cambiamos UseSqlServer por UseNpgsql.
builder.Services.AddDbContext<FunerariaContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------------------------------------------------------------------
// 2. CORS (Permitir que el Frontend en Netlify/Localhost se conecte)
// -----------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()   // Permite cualquier URL (útil para desarrollo)
              .AllowAnyMethod()   // Permite GET, POST, PUT, DELETE
              .AllowAnyHeader();  // Permite cabeceras de autorización
    });
});

// -----------------------------------------------------------------------------
// 3. SEGURIDAD JWT (Validación del Token)
// -----------------------------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddControllers();

// -----------------------------------------------------------------------------
// 4. SWAGGER (Documentación con botón de Candado)
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Funeraria API", Version = "v1" });

    // Definimos que usamos tokens tipo "Bearer"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// 5. MIDDLEWARE (El orden aquí es CRUCIAL)
// -----------------------------------------------------------------------------

// Swagger activo siempre (incluso en producción para que Render funcione bien)
app.UseSwagger();
app.UseSwaggerUI();

// Redirección HTTPS
app.UseHttpsRedirection();

// CORS debe ir ANTES de la Autenticación
app.UseCors("PermitirTodo");

// Autenticación y Autorización
app.UseAuthentication(); // 1. ¿Quién eres?
app.UseAuthorization();  // 2. ¿Tienes permiso?

// Mapeo de controladores
app.MapControllers();

app.Run();