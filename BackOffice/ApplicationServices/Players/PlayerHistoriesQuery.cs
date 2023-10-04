using System;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record PlayerHistoriesQuery(
    string PlayerCode,
    DateTime From,
    DateTime To,
    int Skip = 0);
