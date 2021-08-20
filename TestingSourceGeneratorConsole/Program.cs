using System;

namespace TestingSourceGeneratorConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(SomeClass.SubClass.SomeBuriedEnumIvemade.Geoff.FastToString());
            Console.WriteLine(SomePublicEnumIvemade.Geoan.FastToString());
        }

        
    }

    public class SomeClass
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

    public enum SomePublicEnumIvemade
    {
        Jane,
        Janey,
        Geoan,
        Geoany,
    }

}
