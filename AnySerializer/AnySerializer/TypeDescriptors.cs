﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnySerializer
{
    /// <summary>
    /// Stores a list of defined types used in a serialized data chunk
    /// </summary>
    public class TypeDescriptors
    {
        public static TypeDescriptors None { get { return new TypeDescriptors(); } }

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
        public ushort AddKnownType(TypeSupport type)
        {
            var existingType = Types.FirstOrDefault(x => x.FullName.Equals(type.Type.FullName));
            if (existingType == null)
            {
                // add a new type to the map
                var typeId = _currentTypeId;
                Types.Add(new TypeDescriptor(typeId, type.Type.FullName));
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
        public TypeDescriptor GetTypeDescriptor(ushort typeId)
        {
            return Types.FirstOrDefault(x => x.TypeId == typeId);
        }

        /// <summary>
        /// True if the TypeId is in the type descriptor map
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public bool Contains(ushort typeId)
        {
            return Types.Any(x => x.TypeId == typeId);
        }
    }

    /// <summary>
    /// A type descriptor that defines a Type Id and the full class name of the type
    /// </summary>
    public class TypeDescriptor
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

        public override int GetHashCode()
        {
            return TypeId;
        }

        public override bool Equals(object obj)
        {
            var other = (TypeDescriptor)obj;
            return TypeId == other.TypeId;
        }

        public override string ToString()
        {
            return $"{FullName}";
        }
    }
}