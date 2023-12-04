using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tgs.Core.Infrastructures.Security;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(
    IPasswordHandler passwordHandler,
    ITgsQueries<User> userQueries,
    ITgsQueries<Department> departmentQueries,
    ITgsQueries<Role> roleQueries,
    ITgsCommands<User> userCommands) : BaseContextController
{
    [Route("")]
    [HttpPost]
    [Description("Create a user")]
    public async Task<MessageResponse> CreateAsync([FromBody] CreateUserRequest request)
    {
        var role = await roleQueries.GetSingleAsync(p => p.Id == request.RoleId);

        if (role == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.NotFound);
        }

        if (role.RoleType == RoleType.Admin && Username != "hank.wu")
        {
            return CreateValidResponse<MessageResponse>();
        }

        var usernameExisted = await userQueries.GetQueries()
            .AnyAsync(x => x.Username == request.Username);

        if (usernameExisted)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.User.UsernameExistedAlready);
        }

        if (await departmentQueries.GetSingleAsync(p => p.Id == request.DepartmentId) == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NotFound);
        }

        var (salt, password) = passwordHandler.GenerateCredential(request.Password);

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

        await userCommands.CreateAsync(user);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet(Name = "GetUserList")]
    [Description("Get users")]
    public async Task<PagedResponse<UserModel>> GetUserListAsync(
        [FromServices] IGetService<GetUsersQuery, int> countService,
        [FromServices] IQueryService<GetUsersQuery, UserInfo> queryService,
        [FromQuery] GetUsersRequest request,
        [FromQuery] PageableRequest pageableRequest)
    {
        var paging = new Paging<UserInfo>
        {
            RowCount = pageableRequest.Rows,
            Page = pageableRequest.Page,
        };

        paging.SortByPropertyName(
            string.IsNullOrWhiteSpace(pageableRequest.SortBy)
                ? nameof(UserInfo.Username)
                : pageableRequest.SortBy,
            pageableRequest.IsDesc);

        var query = new GetUsersQuery(
            ResellerId,
            LicenseeId,
            BrandId,
            request.Username,
            request.DepartmentName,
            request.Role,
            request.Status,
            paging);

        var totalCount = await countService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var items = await queryService
            .QueryAsync(query, HttpContext.RequestAborted)
            .Select(u => new UserModel
            {
                Id = u.Id,
                Username = u.Username,
                Name = u.Name,
                RoleId = u.RoleId,
                ResellerId = u.ResellerId,
                LicenseeId = u.LicenseeId,
                BrandId = u.BrandId,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.DepartmentName,
                Role = u.RoleName,
                LastLoginIp = u.LastLoginIp,
                LastLoginTime = u.LastLoginTime,
                Status = u.Status,
                CreatedOn = u.CreatedOn,
            })
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<UserModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = items
        };
    }

    [HttpPut("{userId}")]
    [Description("Update a user")]
    public async Task<MessageResponse> UpdateAsync(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var user = await userQueries.GetSingleAsync(p => p.Id == userId);

        if (user == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.User.NotFound);
        }

        if (await roleQueries.GetSingleAsync(r => r.Id == request.RoleId) == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.NotFound);
        }

        if (await departmentQueries.GetSingleAsync(p => p.Id == request.DepartmentId) == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NotFound);
        }

        user.Name = request.Name;
        user.RoleId = request.RoleId;
        user.DepartmentId = request.DepartmentId;
        user.Status = request.Status;
        user.UpdatedByUsername = Username;
        user.UpdatedOn = DateTimeOffset.Now;

        await userCommands.UpdateAsync(user);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("{userId}/password")]
    [Description("Update user password")]
    public async Task<MessageResponse> UpdatePasswordAsync(Guid userId, [FromBody] UpdatePasswordRequest request)
    {
        var user = await userQueries.GetSingleAsync(p => p.Id == userId);

        if (user == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.User.NotFound);
        }

        var (salt, password) = passwordHandler.GenerateCredential(request.Password);
        user.Password = password;
        user.Salt = salt;
        user.UpdatedByUsername = Username;
        user.UpdatedOn = DateTimeOffset.Now;

        await userCommands.UpdateAsync(user);

        return CreateValidResponse<MessageResponse>();
    }
}
