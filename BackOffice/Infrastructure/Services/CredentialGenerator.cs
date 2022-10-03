using TGS.Core.Infrastructures.Base62;
using Tgs.Core.Infrastructures.Security;

namespace TGS.Services.BackOffice.Infrastructure.Services;

public interface ICredentialGenerator
{
    string GetSecretKey();
    string GetApiKey();
}

public class CredentialGenerator : ICredentialGenerator
{
    private readonly IPasswordHandler _password;

    public CredentialGenerator(IPasswordHandler password)
    {
        _password = password;
    }

    string ICredentialGenerator.GetSecretKey() => GetRandomStringByLength(20);

    string ICredentialGenerator.GetApiKey() => GetRandomStringByLength(40);

    private string GetRandomStringByLength(ushort length) => _password.GenerateRandomSalt(length).ToBase62();
}