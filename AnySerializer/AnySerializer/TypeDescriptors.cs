﻿#if FEATURE_COMPRESSION
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TypeSupport.Extensions;

namespace AnySerializer
{
    /// <summary>
    /// Stores a list of defined types used in a serialized data chunk
    /// </summary>
    public class TypeDescriptors
    {
        /// <summary>
        /// No type descriptors
        /// </summary>
        public static TypeDescriptors None => new TypeDescriptors();

        private ushort _currentTypeId;

        /// <summary>
        /// List of type descriptors
        /// </summary>
        public ICollection<TypeDescriptor> Types { get; set; }

        public TypeDescriptors()
        {
            Types = new List<TypeDescriptor>();
        }

        /// <summary>
        /// Add a type to the type descriptors
        /// </summary>
        /// <param name="type"></param>
        public ushort AddKnownType(Type type)
        {
            var extendedType = type.GetExtendedType();
            var fullName = string.Empty;
            if (!extendedType.IsConcreteType && extendedType.ConcreteType != null)
                fullName = extendedType.ConcreteType.AssemblyQualifiedName;
            else
                fullName = extendedType.Type.AssemblyQualifiedName;
            var existingType = Types.FirstOrDefault(x => x.FullName.Equals(fullName));
            if (existingType == null)
            {
                // add a new type to the map
                var typeId = _currentTypeId;
                Types.Add(new TypeDescriptor(typeId, fullName));
                _currentTypeId++;
                return typeId;
            }

            // return the existing typeId
            return existingType.TypeId;
        }

        /// <summary>
        /// Get a type descriptor by TypeId
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public TypeDescriptor GetTypeDescriptor(ushort typeId) => Types.FirstOrDefault(x => x.TypeId == typeId);

        /// <summary>
        /// True if the TypeId is in the type descriptor map
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public bool Contains(ushort typeId) => Types.Any(x => x.TypeId == typeId);

        /// <summary>
        /// Serialize the type descriptors using LZ4 compression
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            using(var stream = new MemoryStream())
            {
#if FEATURE_COMPRESSION
                // serialize with compression
                using (var lz4Stream = LZ4Stream.Encode(stream))
                {
                    using(var writer = new StreamWriter(lz4Stream))
                    {
                        foreach (var typeDescriptor in Types)
                        {
                            writer.Write($"{typeDescriptor.TypeId}|{typeDescriptor.FullName}\r\n");
                        }
                    }
                }
#else
                // serialize without compression
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    foreach (var typeDescriptor in Types)
                    {
                        var line = $"{typeDescriptor.TypeId}|{typeDescriptor.FullName}\r\n";
                        writer.Write(line);
                    }
                }
#endif
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize the type descriptors using LZ4 compression
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
#if FEATURE_COMPRESSION
                // deserialize with compression
                using (var lz4Stream = LZ4Stream.Decode(stream))
                {
                    using (var reader = new StreamReader(lz4Stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(new [] { '|' }, 2);

                            Types.Add(new TypeDescriptor(ushort.Parse(parts[0]), parts[1]));
                        }
                    }
                }
#else
                // deserialize without compression
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(new [] { '|' }, 2);

                        Types.Add(new TypeDescriptor(ushort.Parse(parts[0]), parts[1]));
                    }
                }
#endif
            }
        }
    }

    /// <summary>
    /// A type descriptor that defines a Type Id and the full class name of the type
    /// </summary>
    public class TypeDescriptor : IEquatable<TypeDescriptor>
    {
        /// <summary>
        /// The type id
        /// </summary>
        public ushort TypeId { get; set; }
        /// <summary>
        /// The full name of the type
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// A type descriptor that defines a Type Id and the full class name of the type
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="fullName"></param>
        public TypeDescriptor(ushort typeId, string fullName)
        {
            TypeId = typeId;
            FullName = fullName;
        }

        public override int GetHashCode() => TypeId;

        public override bool Equals(object obj)
        {
            var other = (TypeDescriptor)obj;
            return TypeId == other.TypeId && FullName.Equals(other.FullName);
        }

        public override string ToString() => $"{FullName}";

        public bool Equals(TypeDescriptor other)
        {
            if (other == null)
                return false;
            return TypeId == other.TypeId && FullName.Equals(other.FullName);
        }
    }
}
