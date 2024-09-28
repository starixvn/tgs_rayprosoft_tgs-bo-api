using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Department;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartmentController(
    ITgsQueries<Department> queries,
    ITgsCommands<Department> commands) : BaseContextController
{
    [HttpPost]
    [BOAuthorize(["System.Departments.Create"])]
    [Description("Create a department")]
    public async Task<MessageResponse> CreateAsync([FromBody] DepartmentRequest request)
    {
        var departmentNameExisted = await queries.GetQueries()
            .Where(request.ResellerId, x => x.ResellerId == request.ResellerId)
            .Where(request.LicenseeId, x => x.LicenseeId == request.LicenseeId)
            .Where(request.BrandId, x => x.BrandId == request.BrandId)
            .AnyAsync(x => x.Name == request.Name);

        if (departmentNameExisted)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NameExists);
        }

        var department = new Department
        {
            Id = Guid.NewGuid(),
            ResellerId = request.ResellerId,
            LicenseeId = request.LicenseeId,
            BrandId = request.BrandId,
            Name = request.Name,
            Description = request.Name,
            CreatedByUsername = Username,
            UpdatedByUsername = Username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await commands.CreateAsync(department);
        return CreateValidResponse<MessageResponse>();
    }

    [HttpDelete("{id}")]
    [BOAuthorize(["System.Departments.Delete"])]
    [Description("Delete a department")]
    public async Task<MessageResponse> DeleteAsync(Guid id)
    {
        var department = await queries.GetSingleAsync(p => p.Id == id, d => d.Include(x => x.Users));
        if (department == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NotFound);
        }

        if (department.Users.Count != 0)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.CannotBeDeleted);
        }

        await commands.DeleteAsync(department);
        return CreateValidResponse<MessageResponse>();
    }

    [HttpGet(Name = "GetDepartmentList")]
    [BOAuthorize(["System.Departments"])]
    [Description("Get departments")]
    public async Task<PagedResponse<DepartmentModel>> GetDepartmentListAsync(
        [FromServices] IGetService<GetDepartmentsQuery, int> countService,
        [FromServices] IQueryService<GetDepartmentsQuery, DepartmentInfo> queryService,
        [FromQuery] GetDepartmentsRequest request,
        [FromQuery] PageableRequest pageableRequest)
    {
        var paging = new Paging<DepartmentInfo>
        {
            RowCount = pageableRequest.ShowAll ? int.MaxValue : pageableRequest.Rows,
            Page = pageableRequest.Page,
        };

        paging.SortByPropertyName(
            string.IsNullOrWhiteSpace(pageableRequest.SortBy)
                ? nameof(DepartmentInfo.Name)
                : pageableRequest.SortBy,
            pageableRequest.IsDesc);

        var query = new GetDepartmentsQuery(
            ResellerId ?? request.ResellerId,
            LicenseeId ?? request.LicenseeId,
            BrandId ?? request.BrandId,
            request.Name,
            paging);

        var totalCount = await countService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var items = await queryService
            .QueryAsync(query, HttpContext.RequestAborted)
            .Select(d => new DepartmentModel
            {
                Id = d.Id,
                ResellerId = d.ResellerId,
                LicenseeId = d.LicenseeId,
                BrandId = d.BrandId,
                ResellerName = d.ResellerName,
                LicenseeName = d.LicenseeName,
                BrandName = d.BrandName,
                Name = d.Name,
            })
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new PagedResponse<DepartmentModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = items
        };
    }

    [HttpPut("{id}")]
    [BOAuthorize(["System.Departments.Update"])]
    [Description("Update a department")]
    public async Task<MessageResponse> UpdateAsync(Guid id, [FromBody] DepartmentRequest request)
    {
        var department = await queries.GetSingleAsync(p => p.Id == id);
        if (department == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NotFound);
        }

        var departmentNameExisted = await queries.GetQueries()
            .Where(x => x.Id != department.Id)
            .Where(department.ResellerId, x => x.ResellerId == department.ResellerId)
            .Where(department.LicenseeId, x => x.LicenseeId == department.LicenseeId)
            .Where(department.BrandId, x => x.BrandId == department.BrandId)
            .AnyAsync(x => x.Name == request.Name);

        if (departmentNameExisted)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Department.NameExists);
        }

        department.Name = request.Name;
        department.UpdatedByUsername = Username;
        department.UpdatedOn = DateTimeOffset.Now;

        await commands.UpdateAsync(department);
        return CreateValidResponse<MessageResponse>();
    }
}