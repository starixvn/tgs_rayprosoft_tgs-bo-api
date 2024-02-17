using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.PlayerOnline;

public record PlayerOnlineModel(
    int OnlineUsers,
    PagedResponse<PlayerOnlineData> Datas);
