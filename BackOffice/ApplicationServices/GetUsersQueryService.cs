using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetUsersQueryService : IQueryService<GetUsersQuery, UserInfo>,
    IGetService<GetUsersQuery, int>
{
    private readonly ITgsQueries<User> _userQueries;

    public GetUsersQueryService(ITgsQueries<User> userQueries)
    {
        _userQueries = userQueries ?? throw new ArgumentNullException(nameof(userQueries));
    }

    public ValueTask<int> GetAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);
        return data.CountAsync(cancellationToken);
    }

    public IAsyncEnumerable<UserInfo> QueryAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);

        return data.ApplyOrderBy(query.Paging).ApplyPaging(query.Paging);
    }

    private IAsyncEnumerable<UserInfo> MakeDataSourceAsync(GetUsersQuery query)
    {
        _ = Enum.TryParse<UserStatus>(query.Status, out var userStatus);

        var usersQuery = _userQueries.GetQueries()
            .Where(query.ResellerId, user => user.Role.ResellerId == query.ResellerId)
            .Where(query.LicenseeId, user => user.Role.LicenseeId == query.LicenseeId)
            .Where(query.BrandId, user => user.Role.BrandId == query.BrandId)
            .Where(query.Username, user => EF.Functions.ILike(user.Username, $"%{query.Username}%"))
            .Where(query.DepartmentName, user => user.Department.Name == query.DepartmentName)
            .Where(query.RoleName, user => user.Role.Name == query.RoleName)
            .Where(query.Status, user => user.Status == userStatus)
            .Select(user => new UserInfo(
                user.Id,
                user.Username,
                user.Name,
                user.RoleId,
                user.Role.ResellerId,
                user.Role.LicenseeId,
                user.Role.BrandId,
                user.DepartmentId,
                user.Role.Name,
                user.Department.Name,
                user.LastLoginIp,
                user.LastLoginTime,
                user.Status.ToString(),
                user.CreatedOn));

        return usersQuery.ToAsyncEnumerable();
    }
}
