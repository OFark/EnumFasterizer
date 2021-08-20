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
            switch(e)
            {
                case SomeEnum.A:
                    return nameof(SomeEnum.A);
                case SomeEnum.B:
                    return nameof(SomeEnum.B);
                case SomeEnum.C:
                    return nameof(SomeEnum.C);
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
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
