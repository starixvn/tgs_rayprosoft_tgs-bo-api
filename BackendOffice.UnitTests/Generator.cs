using System;
using System.Linq;

namespace TGS.Tests.BackendOffice.UnitTests;

public class Generator
{
    private static Random random = new Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string RandomIntString(int length)
    {
        const string chars = "0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static byte[] RandomByteArray(int sizeInKb)
    {
        var rnd = new Random();
        var b = new Byte[sizeInKb]; // convert kb to byte
        rnd.NextBytes(b);
        return b;
    }
}
