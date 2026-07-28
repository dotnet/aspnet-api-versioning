// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.Grpc;

using Google.Protobuf.Reflection;
using Google.Rpc;
using System.Collections.Concurrent;

internal sealed class FileDescriptorPool
{
    private readonly Lock syncRoot = new();
    private readonly HashSet<FileDescriptor> fileDescriptors = [];
    private readonly ConcurrentDictionary<Type, DescriptorBase> typeDescriptorMap = [];

    public FileDescriptorPool() => AddStatusForErrorResponses();

    public void Add( FileDescriptor fileDescriptor )
    {
        using ( syncRoot.EnterScope() )
        {
            AddFileDescriptor( fileDescriptor );
        }
    }

    public DescriptorBase? Find( Type type )
    {
        typeDescriptorMap.TryGetValue( type, out var value );
        return value;
    }

    private void AddStatusForErrorResponses() => AddFileDescriptor( StatusReflection.Descriptor );

    private void AddFileDescriptor( FileDescriptor fileDescriptor )
    {
        var cyclical = !fileDescriptors.Add( fileDescriptor );

        if ( cyclical )
        {
            return;
        }

        // enums
        foreach ( var descriptor in fileDescriptor.EnumTypes )
        {
            typeDescriptorMap[descriptor.ClrType] = descriptor;
        }

        // messages
        foreach ( var descriptor in fileDescriptor.MessageTypes )
        {
            AddMessageDescriptor( descriptor );
        }

        // imports
        foreach ( var dependency in fileDescriptor.Dependencies )
        {
            AddFileDescriptor( dependency );
        }
    }

    private void AddMessageDescriptor( MessageDescriptor messageDescriptor )
    {
        // type is null for map entry message types
        if ( messageDescriptor.ClrType != null )
        {
            typeDescriptorMap[messageDescriptor.ClrType] = messageDescriptor;
        }

        foreach ( var enumDescriptor in messageDescriptor.EnumTypes )
        {
            typeDescriptorMap[enumDescriptor.ClrType] = enumDescriptor;
        }

        foreach ( var nestedMessageDescriptor in messageDescriptor.NestedTypes )
        {
            AddMessageDescriptor( nestedMessageDescriptor );
        }
    }
}