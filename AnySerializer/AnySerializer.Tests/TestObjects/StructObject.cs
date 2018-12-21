using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AnySerializer.Tests.TestObjects
{
    [StructLayout(LayoutKind.Auto)]
    public struct StructObject
    {
        public int Id { get; }
        public int Value { get; }

        public StructObject(int id, int value)
        {
            Id = id;
            Value = value;
        }
    }
}
