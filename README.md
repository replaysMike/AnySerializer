# AnySerializer

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
var originalObject = new SomeComplexTypeWithDeepStructure();
var bytes = Serializer.Serialize();
var restoredObject = Serializer.Deserialize<SomeComplexTypeWithDeepStructure>();

```

### Ignoring Properties/Fields

Ignoring fields/properties is as easy as using any of the following standard ignores: `[IgnoreDataMember]`, `[NonSerializable]` and `[JsonIgnore]`. Note that `[NonSerializable]` only works on fields, for properties (and/or fields) use `[IgnoreDataMember]`.

