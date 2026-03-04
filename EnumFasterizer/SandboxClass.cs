using System;

namespace EnumFasterizer
{
    /// <summary>
    /// This is a solid copy of the class I'm attempting to recreate
    /// </summary>
    public static partial class SandboxClass
    {
        public static string FastToString(this SomeEnum e)
        {
            return e switch
            {
                SomeEnum.A => nameof(SomeEnum.A),
                SomeEnum.B => nameof(SomeEnum.B),
                SomeEnum.C => nameof(SomeEnum.C),
                _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
            };
        }
    }

    /// <summary>
    /// This is just an enum to use as an example
    /// </summary>
    public enum SomeEnum
    {
        A,
        B,
        C
    }
}
