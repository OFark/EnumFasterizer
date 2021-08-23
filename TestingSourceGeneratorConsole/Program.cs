using System;

namespace TestingSourceGeneratorConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(SomeClass.SubClass.SomeBuriedEnumIvemade.Geoff.FastToString());
            Console.WriteLine(SomePublicEnumIdmade.Geoan.FastToString());
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

    public enum SomePublicEnumIdmade
    {
        Jane,
        Janey,
        Geoan,
        Geoany,
    }

}
