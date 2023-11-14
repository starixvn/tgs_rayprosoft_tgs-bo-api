using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.Core.Infrastructures.Security;
using TGS.Apis.BackendOffice.DataService.Repositories;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Extensions;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
SqlMapper.AddTypeMap(typeof(DateOnly), DbType.Date);

var builder = Host.CreateApplicationBuilder(args);
{
    _ = builder.Services
        .AddTgsRepository(builder.Configuration, "Tgs")
        .AddSecurity();
}

var app = builder.Build();

await app.StartAsync();

var passwordHandler = app.Services.GetRequiredService<IPasswordHandler>();
var dbContext = app.Services.GetRequiredService<TgsRepository>();

var (salt, password) = passwordHandler.GenerateCredential(builder.Configuration.GetValue<string>("BO:Pass")!);

var user = new User
{
    Id = GuidExtensions.NewSequentialGuid(),
    Username = builder.Configuration.GetValue<string>("BO:User"),
    Name = builder.Configuration.GetValue<string>("BO:UserName"),
    Password = password,
    Salt = salt,
    RoleId = builder.Configuration.GetValue<Guid>("BO:RoleId"),
    DepartmentId = builder.Configuration.GetValue<Guid>("BO:DepartmentId"),
    TimeZone = "+0800",
    CreatedByUsername = "system",
    UpdatedByUsername = "system",
    CreatedOn = DateTimeOffset.UtcNow,
    UpdatedOn = DateTimeOffset.UtcNow
};

await dbContext.Users.AddAsync(user);
await dbContext.SaveChangesAsync();
