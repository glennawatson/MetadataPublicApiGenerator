using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleInternalClassOnly
{
    internal class Dummy
    {
    }
}

namespace ExampleInternalStructOnly
{
    internal struct Dummy
    {
    }
}

namespace ExampleInternalPublicClasses
{
    public class DummyPublic
    {
    }

    internal class DummyInternal
    {
    }
}

namespace ExampleInternalPublicStructs
{
    public struct DummyPublic
    {
    }

    internal struct DummyInternal
    {
    }
}

namespace ExampleInternalPublicStructsClasses
{
    public class DummyPublicClass
    {
    }

    internal class DummyInternalClass
    {
    }

    public struct DummyPublicStruct
    {
    }

    internal struct DummyInternalStruct
    {
    }
}

namespace ExamplePublicEnum
{
    public enum Test
    {
        A = 0,
        B = 255
    }
}