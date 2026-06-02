using System;

namespace TestingSourceGeneratorConsole;

public class Program
{
    static void Main(string[] _)
    {
        Console.WriteLine(SomeClass.SubClass.SomeBuriedEnumIvemade.Geoff.FastToString());
        Console.WriteLine(SomePublicEnumIdmadeEarlier.Geoan.FastToString());
        Console.WriteLine(SomeInternalEnum.Jeffers.FastToString());
    }


    private enum SomePrivateEnum
    {
        Jeff,
        Geoff,
        Geoffrey,
        Jeffers
    }
}

class SomeClass
{
    public class SubClass
    {
        public enum SomeBuriedEnumIvemade
        {
            Jeff,
            Geoff,
            Geoffrey,
            Jeffers
        }
    }
}

public enum SomePublicEnumIdmadeEarlier
{
    Jane,
    [Obsolete]
    Janey,
    Geoan,
    [Obsolete("Please don't use")]
    Geoany,
}

internal enum SomeInternalEnum
{
    Jeff,
    Geoff,
    Geoffrey,
    Jeffers
}
