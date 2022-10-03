using MediatR;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public record GetSubGameProviderRequest(string Code) : IRequest<GetSubGameProviderResponse>;