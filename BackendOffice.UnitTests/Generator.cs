namespace TGS.Tests.BackendOffice.UnitTests;

public class Generator
{
    private static readonly Random _Random = new();

    public static byte[] RandomByteArray(int sizeInKb)
    {
        var rnd = new Random();
        var b = new byte[sizeInKb]; // convert kb to byte
        rnd.NextBytes(b);
        return b;
    }

    public static string RandomIntString(int length)
    {
        const string chars = "0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_Random.Next(s.Length)]).ToArray());
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_Random.Next(s.Length)]).ToArray());
    }
}
