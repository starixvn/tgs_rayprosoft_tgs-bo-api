using System;
using System.Text.Json;
using Tgs.Agent.Client.Models.Enums;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AgentHistoryContentViewModel
{
    public EventAction Action { get; set; }

    public JsonDocument ChangeSets { get; set; }

    public DateTime CreatedOn { get; set; }

    public string ModifyUsername { get; set; }
}
