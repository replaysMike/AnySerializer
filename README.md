# AnySerializer

[![nuget](https://img.shields.io/nuget/v/AnySerializer.svg)](https://www.nuget.org/packages/AnySerializer/)
[![nuget](https://img.shields.io/nuget/dt/AnySerializer.svg)](https://www.nuget.org/packages/AnySerializer/)
[![Build status](https://ci.appveyor.com/api/projects/status/gfwjabg1pta7em94?svg=true)](https://ci.appveyor.com/project/MichaelBrown/anyserializer)

A CSharp library that can binary serialize any object quickly and easily. No attributes/decoration required!

That's right, no need for `[Serializable]` or any other custom attributes on your classes!

## Description

AnySerializer was built for software applications that make manual serialization difficult, or time consuming to decorate and design correctly. Other libraries require custom attributes to define serialization contracts, or fail at more complicated scenarios that involve interfaces, delegates and events defined. That's where AnySerializer shines! It literally is an anything in, anything out binary serializer.

## Installation
Install AnySerializer from the Package Manager Console:
```
PM> Install-Package AnySerializer
```

## Usage

```csharp
using AnySerializer;

var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject);
var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>(bytes);

```

### Ignoring Properties/Fields

Ignoring fields/properties is as easy as using any of the following standard ignores: `[IgnoreDataMember]`, `[NonSerializable]` and `[JsonIgnore]`. Note that `[NonSerializable]` only works on fields, for properties (and/or fields) use `[IgnoreDataMember]`.

### Providing custom type mappings

If you find you need to map interfaces to concrete types that are contained in different assemblies, you can add add custom type mappings:

```csharp
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject);

var typeMaps = TypeRegistry.Configure((config) => {
  config.AddMapping<ICustomInterfaceName, ConcreteClassName>();
  config.AddMapping<ICustomer, Customer>();
});

var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>(bytes, typeMaps);
```

or alternatively, a type factory for creating empty objects:

```csharp
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject);

var typeMaps = TypeRegistry.Configure((config) => {
  config.AddFactory<ICustomInterfaceName, ConcreteClassName>(() => new ConcreteClassName());
});

var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>(bytes, typeMaps);
```

and an alternate form for adding one-or-more mappings:

```csharp
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject);

var typeMap = TypeRegistry.For<ICustomInterfaceName>()
                .Create<ConcreteClassName>();

var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>(bytes, typeMap);
```

or single type one-or-more factories:

```csharp
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject);

var typeMap = TypeRegistry.For<ICustomInterfaceName>()
                .CreateUsing<ConcreteClassName>(() => new ConcreteClassName());

var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>(bytes, typeMap);
```

### Complicated scenarios - Embedded Type Descriptors to the rescue!

There are some scenarios that cause grief when serializing certain types. Things like abstract interfaces and anonymous types require information about how to serialize them. To solve this, you can choose to embed type information for these scenarios which will increase the size of the serialized data slightly - which is optimized and compressed so it's not that much data.

To embed type descriptors in the serialized data:

```csharp
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject, SerializerOptions.EmbedTypes);
var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>(bytes);
```

What does it do? Essentially what is going on here is we store a reference to the assembly and type which tells AnySerializer how to restore the data when deserializing. Only types that are interfaces and anonymous types are stored and concrete classes are ignored. When this _isn't_ applied AnySerializer can still try to figure out what to do, but it doesn't guarantee that it will succeed if types are contained in assemblies it isn't aware of, or where there are multiple concrete classes available for an interface. 

### Validating binary data

A validator is provided for verifying if a serialized object contains valid deserializable data that has not been corrupted:

```csharp
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize(originalObject);
var isValid = Serializer.Validate(bytes);
Assert.IsTrue(isValid);
```

### Extensions

You can use the extensions to perform serialization/deserialization:

```csharp
using AnySerializer.Extensions;

var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = originalObject.Serialize();
var restoredObject = bytes.Deserialize<SomeComplexTypeWithDeepStructure>();
```

### Scenarios supported

- [x] All basic types, enums, generics, collections
- [x] Read-only types
- [x] Circular references
- [x] Ignore attributes on unwanted fields/properties
- [x] Constructorless classes
- [x] Anonymous types
- [x] Ignoring of delegates and events, other non-serializable types
- [x] Resolving abstract interfaces to concrete types
- [x] Manually specifying custom type mappings through the registry
- [x] Embedded type descriptors
- [x] Data validator
- [x] Custom collections
- [ ] High performance testing and optimization

### Other applications

To see differences between two serialized objects you can use [AnyDiff](https://github.com/replaysMike/AnyDiff) on your copied object:

```csharp
using AnyDiff;

var object1 = new MyComplexObject(1, "A string");
var object1bytes = Serializer.Serialize(object1);
var object2 = Serializer.Deserialize<MyComplexObject>(object1bytes);

object2.Id = 100;

// view the changes between them
var diff = object1.Diff(object2);
Assert.AreEqual(diff.Count, 1);
```

If you need a way to copy an object that doesn't involve serialization, try [AnyClone](https://github.com/replaysMike/AnyClone) which is a pure reflection based cloning library!
