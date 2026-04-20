using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using voucherMicroservice.Data;
using voucherMicroservice.Model;
using voucherMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var conString = builder.Configuration.GetConnectionString("DefaultConnection");
var conStringBackup = builder.Configuration.GetConnectionString("BackupConnection");


//original
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(conString, ServerVersion.AutoDetect(conString)));

//backup
// builder.Services.AddDbContext<DataContextBackup>(options =>
//     options.UseMySql(conStringBackup, ServerVersion.AutoDetect(conStringBackup)));

// builder.Services.AddDbContext<DataContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new ServerVersion.AutoDetect(c)));
builder.Services.AddControllers();

//service deploy
builder.Services.AddScoped<IFloatingService, FloatingService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IStaffListService, StaffListService>();
builder.Services.AddScoped<ResponseCustomModel<string>>();
builder.Services.AddScoped<ResponseCustomModel<List<PayHistory>>>();
builder.Services.AddScoped<ResponseCustomModel<PayHistory>>();
builder.Services.AddScoped<ResponseCustomModel<List<Seller>>>();
builder.Services.AddScoped<IPayHistoryService, PayHistoryService>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUsermanagementService, UsermanagementService>();
builder.Services.AddScoped<IDuplicateSolverService, DuplicateEntrySolverService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddHttpContextAccessor();

//JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jswt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.UseCors(x => x
            .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());

app.Run();
