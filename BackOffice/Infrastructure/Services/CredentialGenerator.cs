using Tgs.Core.Infrastructures.Security;
using TGS.Core.Infrastructures.Base62;

namespace TGS.Services.BackOffice.Infrastructure.Services;

public interface ICredentialGenerator
{
    string GetApiKey();

    string GetSecretKey();
}

public class CredentialGenerator(IPasswordHandler password) : ICredentialGenerator
{
    private readonly IPasswordHandler _password = password;

    string ICredentialGenerator.GetApiKey() => GetRandomStringByLength(40);

    string ICredentialGenerator.GetSecretKey() => GetRandomStringByLength(20);

    private string GetRandomStringByLength(ushort length) => _password.GenerateRandomSalt(length).ToBase62();
}
