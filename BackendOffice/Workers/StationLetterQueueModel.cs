using System;

namespace TGS.Apis.BackendOffice.Workers;

public record StationLetterQueueModel(
    Guid BrandId,
    Guid StationLetterId);
