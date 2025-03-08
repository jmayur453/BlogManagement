using BlogManagement.Helpers;
using BlogManagement.Models;
using BlogManagement.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BlogApplicationDBContext>(options =>
    options.UseSqlServer(connectionString));
// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy => policy.WithOrigins("http://localhost:4200")  // Allow requests from Angular dev server
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});
// Add services to the container.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddControllers();

builder.Services.AddScoped<UserHelper>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlogManagement", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter '{your_token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalhost");
app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#region Manual Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlogApplicationDBContext>();
    context.Database.Migrate(); // Ensures the database is created & migrated

    if (!context.Categories.Any()) // Check if data already exists
    {
        context.Categories.AddRange(new List<Category>
        {
            new Category { Name ="Web Development (Angular, React, Vue)"},
            new Category { Name ="Microservices Architecture"},
            new Category { Name ="Database & SQL Optimization"},
            new Category { Name ="DevOps & CI/CD"},
            new Category { Name ="Cybersecurity & Data Protection"},
        });
        context.SaveChanges();
    }
    if (!context.UserDetails.Any(x => x.Email == "test")) // Check if data already exists
    {
        context.UserDetails.AddRange(new List<UserDetail>
        {
            new UserDetail { Email ="test",FullName="Test",MobileNo="1234567890", Password="test"},
        });
        context.SaveChanges();
    }
    if (!context.UserBlogDetails.Any()) // Check if data already exists
    {
        var user = context.UserDetails.First();
        var category1 = context.Categories.First(x => x.Name == "Web Development (Angular, React, Vue)");
        var category2 = context.Categories.First(x => x.Name == "Microservices Architecture");
        var category3 = context.Categories.First(x => x.Name == "Database & SQL Optimization");
        var category4 = context.Categories.First(x => x.Name == "DevOps & CI/CD");
        var category5 = context.Categories.First(x => x.Name == "Cybersecurity & Data Protection");
        context.UserBlogDetails.AddRange(new List<UserBlogDetail>
        {
            new UserBlogDetail{ CategoryId = category1.Id,Content="Content 1",Title="Title 1",UserDetailId=user.Id,ImageURL="https://s3-alpha-sig.figma.com/img/2c71/d319/63450c58fe599c40a68f1bf74c09b3de?Expires=1742169600&Key-Pair-Id=APKAQ4GOSFWCW27IBOMQ&Signature=giOQ8PTbv5AW8qgE~U7bG-pNTMvKdixgxdlMORHBbQqlzHU~bce6g4FMqYeKveIbk4ltBF9AAGqcLvYMMeWwvz1UUo7wBBa7yROc8GM8f6KOEngaibhTXVoDWVN2dhp4HWdGbdg0wgD6gDnHz8ty0WZBCdARnepMfeNdPzLVGmWQcUUhHPQmCZMdTRVsGuDPq5km7DtgJRTWAZ-PwKqbi9W9AaPsKKvGQ976DEHeSitfnJwQ-5347TY7WLFNiRPYnreev4F8bF7Fq6hLiRrS0r8HMsuLYK0sfYuo3k-sG3F5ZbUvdBDNZR9JMQfeszseNLgA3oZHiH4BqS1qy1XphA__" },
            new UserBlogDetail{ CategoryId = category2.Id,Content="Content 2",Title="Title 2",UserDetailId=user.Id,ImageURL="https://s3-alpha-sig.figma.com/img/908f/6e6d/adefff9c6fad99774e0aa7808b2270ab?Expires=1742169600&Key-Pair-Id=APKAQ4GOSFWCW27IBOMQ&Signature=OItOeb1~kb1gCUJgdvc~SH63oiNCnSDrQVr7BpzDxYkZ4dUbKi71~263z7kLe~mROyA1uwGJuA7tKw-8wlWMaDap7Y-DusGBbUGszO~~~W9GTiZMF9AayGr2MjQQ2lWXcpVckifDTlrRvlXG0BquQ8kccedgh2FNCHOzN3XDe6MGU9dTMaKaIIwoqrjlXzbU4JCypiyJuqMnFxwOA4260fIuOHFGD471lJSDjB--Pjg6kHfyreodNbfjxN1Eur2BsLvq2rUe2qbpvo3gKYl00YnSWKRTGfjHU5MvCK1iRx47ndyaHUc7tU7s61qfKOARxd6XSwGyFpSXvz~3QSM62w__" },
            new UserBlogDetail{ CategoryId = category3.Id,Content="Content 3",Title="Title 3",UserDetailId=user.Id,ImageURL="https://s3-alpha-sig.figma.com/img/aa0e/63b4/b72dafa20ebf705cb3408f9d3a4343ef?Expires=1742169600&Key-Pair-Id=APKAQ4GOSFWCW27IBOMQ&Signature=pSwT-wAFUG1580Lyq~7BZHGtlMwcE8EvX94ecifl0JAl949cxlhIWw6mTIMZVtWugeo-3BIeGMsUfyqTQKJ7uA4MpBEWI2EgTlysa12AtshfLQr8NYECVlcr~7yEUuIkTcw2Ajlm5XzuUBSHY~WKIM341M11a6MypXS8x7f6QuEiZQiaCOqBGlEzFGQcpSGvpTRSxRP4Vd-kOLwfkZEjyGokKMQY4ZR4juXFUdYXVDTDo0AGe8hA0fAz1MPBOrgknZ4DLfzIfYk0LGwCzOTwC5UK5zRvpi-eHlW69q4x2qkJxOIaYK4H~lX6SrbChywF57IAb9q2umahdwiRpcfz-w__" },
            new UserBlogDetail{ CategoryId = category4.Id,Content="Content 4",Title="Title 4",UserDetailId=user.Id,ImageURL="https://s3-alpha-sig.figma.com/img/950f/4305/76ceafadb9f758a56d19a086f7505a80?Expires=1742169600&Key-Pair-Id=APKAQ4GOSFWCW27IBOMQ&Signature=Vv22qkYNcVMAmZ-Qx0xRMlEPt8iGYssyYR72GKPgICMtSEhVaRA~TXjXnVngmb6sMe~gvfImoaAwmxoT5jpY6ueMLHegsn4YM84eyFJMyMchpNNN66EsGJHV5FOZkLhpiKCxxTf2f3-hacoY0T72F7w7MDFR-El83gSDx~3~Xyva~eZZNqaHEMbqBFROSTKlTWLIL9ruqzd1BUYnOlmkZeXWas4OSuBM7jlBzPsc6gRRLAzm7zd~3iR10y0EqnJOOMCM~~ZwbcKGK2sCCBG5eV9leUQRHIxGxyc3GQ0gG7r8tvDkOBf5bIbKf0v6AeWpHLkkgnP-8745P2SwKA940w__" },
            new UserBlogDetail{ CategoryId = category5.Id,Content="Content 5",Title="Title 5",UserDetailId=user.Id,ImageURL="https://s3-alpha-sig.figma.com/img/1155/4089/6b913ddf85158d3f3246fba7da982400?Expires=1742169600&Key-Pair-Id=APKAQ4GOSFWCW27IBOMQ&Signature=V6E8os31soqztTGjzRLH~MOj1ApzKP2LBC39DR6Z2nWmxSng2ez44PAeiQz1s9lsRtkWH8Ux9I69t8ZKcwyTvRqWkzZph5pw7dc0tBiUJ48S-BZtOP3XZjhfeEjV12gU3oN7DfsR57sIbVYMCqvevJYS~2Mzi1grmztPmUp0IG-g3PEKW3Hzir61l2Py-DMOWPMtmxOqfW0NuDZzR-ouRLlcCf0AYDyaHmoCXvjAFJTedfYK1BHq8Xo6RJLQLynhlp2eH2OSjBphbrMi-IqbZ-dQ369CQ52mqyvkTgKpODI9RIrRoT7dih48YxHrBP0Va78r88xA0GVKncwVFHmMMw__" },
        });
        context.SaveChanges();
    }
}
#endregion

app.Run();
