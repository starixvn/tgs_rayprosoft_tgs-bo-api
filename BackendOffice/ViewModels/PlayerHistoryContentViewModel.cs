using System;
using System.Text.Json;
using Tgs.PlayerService.Client;

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerHistoryContentViewModel
{
    public EventAction Action { get; set; }

    public JsonDocument ChangeSets { get; set; }

    public DateTime CreatedOn { get; set; }

    public string ModifyUsername { get; set; }
};
