using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using Tgs.Core.Infrastructures.Security;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : BaseContextController
{
    private readonly ITgsQueries<Department> _departmentQueries;
    private readonly IPasswordHandler _passwordHandler;
    private readonly ITgsQueries<Role> _roleQueries;
    private readonly ITgsCommands<User> _userCommands;
    private readonly ITgsQueries<User> _userQueries;

    public UserController(
        IPasswordHandler passwordHandler,
        ITgsQueries<User> userQueries,
        ITgsQueries<Department> departmentQueries,
        ITgsQueries<Role> roleQueries,
        ITgsCommands<User> userCommands)
    {
        _passwordHandler = passwordHandler;
        _userQueries = userQueries;
        _departmentQueries = departmentQueries;
        _roleQueries = roleQueries;
        _userCommands = userCommands;
    }

    [Route("")]
    [HttpGet]
    [Description("Get users")]
    public Task<PagedResponse<UserModel>> GetListAsync([FromQuery] GetUsersRequest request) =>
        _userQueries.GetQueries()
            .Where(ResellerId, r => r.Role.ResellerId == ResellerId)
            .Where(LicenseeId, l => l.Role.LicenseeId == LicenseeId)
            .Where(BrandId, l => l.Role.BrandId == BrandId)
            .Where(request.Username, p => EF.Functions.ILike(p.Username, $"%{request.Username}%"))
            .Select(user => new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                RoleId = user.RoleId,
                ResellerId = user.Role.ResellerId,
                LicenseeId = user.Role.LicenseeId,
                BrandId = user.Role.BrandId,
                DepartmentId = user.DepartmentId,
                Role = user.Role.Name,
                DepartmentName = user.Department.Name,
                LastLoginIp = user.LastLoginIp,
                LastLoginTime = user.LastLoginTime,
                Status = user.Status.ToString(),
                CreatedOn = user.CreatedOn
            })
            .CreatePagedResponseAsync(request);

    [Route("")]
    [HttpPost]
    [Description("Create a user")]
    public async Task<MessageResponse> CreateAsync([FromBody] CreateUserRequest request)
    {
        var role = await _roleQueries.GetSingleAsync(p => p.Id == request.RoleId);

        if (role == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.NotFound);
        }

        var usernameExisted = await _userQueries.GetQueries()
            .AnyAsync(x => x.Username == request.Username);

        if (usernameExisted)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.User.UsernameExistedAlready);
        }

        if (await _departmentQueries.GetSingleAsync(p => p.Id == request.DepartmentId) == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NotFound);
        }

        var (salt, password) = _passwordHandler.GenerateCredential(request.Password);

        var user = new User
        {
            Id = GuidExtensions.NewSequentialGuid(),
            Username = request.Username,
            Name = request.Name,
            Password = password,
            Salt = salt,
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,
            TimeZone = request.TimeZone,
            CreatedByUsername = Username,
            UpdatedByUsername = Username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await _userCommands.CreateAsync(user);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("{userId}")]
    [Description("Update a user")]
    public async Task<MessageResponse> UpdateAsync(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var user = await _userQueries.GetSingleAsync(p => p.Id == userId);

        if (user == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.User.NotFound);
        }

        if (await _roleQueries.GetSingleAsync(r => r.Id == request.RoleId) == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.NotFound);
        }

        if (await _departmentQueries.GetSingleAsync(p => p.Id == request.DepartmentId) == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NotFound);
        }

        user.Name = request.Name;
        user.RoleId = request.RoleId;
        user.DepartmentId = request.DepartmentId;
        user.Status = request.Status;
        user.UpdatedByUsername = Username;
        user.UpdatedOn = DateTimeOffset.Now;

        await _userCommands.UpdateAsync(user);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("{userId}/password")]
    [Description("Update user password")]
    public async Task<MessageResponse> UpdatePasswordAsync(Guid userId, [FromBody] UpdatePasswordRequest request)
    {
        var user = await _userQueries.GetSingleAsync(p => p.Id == userId);

        if (user == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.User.NotFound);
        }

        var (salt, password) = _passwordHandler.GenerateCredential(request.Password);
        user.Password = password;
        user.Salt = salt;
        user.UpdatedByUsername = Username;
        user.UpdatedOn = DateTimeOffset.Now;

        await _userCommands.UpdateAsync(user);

        return CreateValidResponse<MessageResponse>();
    }
}