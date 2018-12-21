using System.Runtime.InteropServices;

namespace AnySerializer.Tests.TestObjects
{
    [StructLayout(LayoutKind.Auto)]
    public struct RecursiveStructObject
    {
        public int Id { get; }
        // static members should not be recursed in structs
        public static readonly RecursiveStructObject MinId = new RecursiveStructObject(0);
        public static readonly RecursiveStructObject MaxId = new RecursiveStructObject(100);

        public RecursiveStructObject(int id)
        {
            Id = id;
        }
    }
}
