﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserManagementSystem;
using UserManagementSystem.Data;
using UserManagementSystem.Filters;
using UserManagementSystem.Models.ResponseModel;
using UserManagementSystem.Models.UserModel;
using UserManagementSystem.Repositories.GenericRepositories;
using UserManagementSystem.Repositories.RolesRepositories;
using UserManagementSystem.Repositories.UserCourseRepository;
using UserManagementSystem.Repositories.UserRepositories;
using UserManagementSystem.Services.AuthenticationService;
using UserManagementSystem.Services.AuthenticationService.EmailService;
using UserManagementSystem.Services.CourseService;
using UserManagementSystem.Services.RoleService;
using UserManagementSystem.Services.UserService;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCourseRepository, UserCourseRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<ContentTypeValidationMiddleware>();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddSwaggerGen(c =>
{
    // UI for authorize Token .
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

    // Add JWT support for swagger apis.
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Enter JWT Bearer token here (without 'Bearer' prefix)",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            jwtSecurityScheme,
            Array.Empty<string>()
        }
    });
});


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//  Identity setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

//  JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettingsSection["Issuer"],
        ValidAudience = jwtSettingsSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettingsSection["Token"])
        )
    };
});
// Setup for Filters.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequestValidationFilter>();
});
// Unique Email Validation.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
});
var app = builder.Build();

    app.UseMiddleware<ContentTypeValidationMiddleware>();
    // middleware to return 401 and 405 responses
    app.UseStatusCodePages(async context =>
    {
        var response = context.HttpContext.Response;
        response.ContentType = "application/json";
        if (response.StatusCode == StatusCodes.Status405MethodNotAllowed)
        {
            var errorResponse = new ApiResponse<object>(
                null,
                MessagesConstants.InvalidHttp,
                false
            );
            await response.WriteAsJsonAsync(errorResponse);
        }
        else if (response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            var errorResponse = new ApiResponse<object>(
                null,
                MessagesConstants.UnauthorizedToken,
                false
            );
            await response.WriteAsJsonAsync(errorResponse);
        }
    });
    // Swagger setup
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    // Authentication and Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.Run();

