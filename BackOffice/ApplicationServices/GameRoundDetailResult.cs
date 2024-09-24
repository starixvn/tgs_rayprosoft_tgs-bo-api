using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices
{
    public record GameRoundDetailResult
    {
        public List<GameRoundDetailData> Data { get; set; }
    }

    public record GameRoundDetailData
    {
        public string Type { get; set; }

        public object Value { get; set; }
    }
}