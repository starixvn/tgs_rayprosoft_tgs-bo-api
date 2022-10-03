using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerOnline;

public record PlayerOnlineModel(
    int OnlineUsers,
    PagedResponse<PlayerOnlineData> Datas
    );
