﻿namespace FsPickler
    
    open System
    open System.IO
    open System.Collections
    open System.Collections.Generic
    open System.Runtime.Serialization

    open FsPickler.PicklerUtils

    type FsPickler private (cache : PicklerCache) =

        let name = cache.Name
        let resolver = cache :> IPicklerResolver
        
        /// initializes an instance that resolves picklers from a global cache
        new () = new FsPickler(PicklerCache.GetDefaultInstance())
        /// initializes a new pickler cache that resolves picklers using custom rules
        new (registry : CustomPicklerRegistry) = new FsPickler(PicklerCache.FromPicklerRegistry registry)

        /// Human-readable name for the pickler cache
        member __.Name = name
        /// Identifier of the cache instance used by the serializer.
        member __.UUId = resolver.UUId

        /// <summary>Serialize value to the underlying stream.</summary>
        /// <param name="stream">target stream.</param>
        /// <param name="value">value to be serialized.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary writer.</param>
        /// <param name="leaveOpen">Leave underlying stream open when finished. Defaults to true.</param>
        member __.Serialize<'T>(stream : Stream, value : 'T, ?streamingContext : obj, ?encoding, ?leaveOpen) : unit =
            use writer = new Writer(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            writer.Write<'T> value

        /// <summary>Serialize value to the underlying stream using given pickler.</summary>
        /// <param name="pickler">pickler used for serialization.</param>
        /// <param name="stream">target stream.</param>
        /// <param name="value">value to be serialized.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.Serialize<'T>(pickler : Pickler<'T>, stream : Stream, value : 'T, ?streamingContext : obj, ?encoding, ?leaveOpen) : unit =
            do checkPicklerCompat resolver.UUId pickler
            use writer = new Writer(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            writer.Write(pickler, value)

        /// <summary>Serialize object of given type to the underlying stream.</summary>
        /// <param name="valueType">type of the given object.</param>
        /// <param name="stream">target stream.</param>
        /// <param name="value">value to be serialized.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.Serialize(valueType : Type, stream : Stream, value : obj, ?streamingContext : obj, ?encoding, ?leaveOpen) : unit =
            use writer = new Writer(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            let pickler = resolver.Resolve valueType
            pickler.ManagedWrite writer value

        /// <summary>Serialize object to the underlying stream using given pickler.</summary>
        /// <param name="pickler">untyped pickler used for serialization.</param>
        /// <param name="stream">target stream.</param>
        /// <param name="value">value to be serialized.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.Serialize(pickler : Pickler, stream : Stream, value : obj, ?streamingContext : obj, ?encoding, ?leaveOpen) : unit =
            use writer = new Writer(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            pickler.ManagedWrite writer value

        /// <summary>Deserialize value of given type from the underlying stream.</summary>
        /// <param name="stream">source stream.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.Deserialize<'T> (stream : Stream, ?streamingContext : obj, ?encoding, ?leaveOpen) : 'T =
            use reader = new Reader(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            reader.Read<'T> ()

        /// <summary>Deserialize value of given type from the underlying stream, using given pickler.</summary>
        /// <param name="pickler">pickler used for serialization.</param>
        /// <param name="stream">source stream.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.Deserialize<'T> (pickler : Pickler<'T>, stream : Stream, ?streamingContext : obj, ?encoding, ?leaveOpen) : 'T =
            do checkPicklerCompat resolver.UUId pickler
            use reader = new Reader(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            reader.Read<'T> pickler

        /// <summary>Deserialize object of given type from the underlying stream.</summary>
        /// <param name="valueType">anticipated value type.</param>
        /// <param name="stream">source stream.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.Deserialize (valueType : Type, stream : Stream, ?streamingContext : obj, ?encoding, ?leaveOpen) : obj =
            use reader = new Reader(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            let pickler = resolver.Resolve valueType
            pickler.ManagedRead reader

        /// <summary>Deserialize object from the underlying stream using given pickler.</summary>
        /// <param name="pickler">untyped pickler used for deserialization.</param>
        /// <param name="stream">source stream.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        /// <return>number of elements written to the stream.</return>
        member __.Deserialize (pickler : Pickler, stream : Stream, ?streamingContext : obj, ?encoding, ?leaveOpen) : obj =
            use reader = new Reader(stream, resolver, ?streamingContext = streamingContext, ?encoding = encoding, ?leaveOpen = leaveOpen)
            pickler.ManagedRead reader

        /// <summary>Serialize a sequence of objects to the underlying stream.</summary>
        /// <param name="stream">target stream.</param>
        /// <param name="sequence">input sequence.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        member __.SerializeSequence<'T>(stream : Stream, sequence:seq<'T>, ?streamingContext : obj, ?encoding, ?leaveOpen) : int =
            use writer = new Writer(stream, resolver, ?encoding = encoding, ?leaveOpen = leaveOpen)
            let pickler = resolver.Resolve<'T> ()
            writer.WriteSequence(pickler, sequence)

        /// <summary>Serialize a sequence of objects to the underlying stream.</summary>
        /// <param name="elementType">element type used in sequence.</param>
        /// <param name="stream">target stream.</param>
        /// <param name="sequence">input sequence.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        /// <return>number of elements written to the stream.</return>
        member __.SerializeSequence(elementType : Type, stream : Stream, sequence : IEnumerable, ?streamingContext : obj, ?encoding, ?leaveOpen) : int =
            use writer = new Writer(stream, resolver, ?encoding = encoding, ?leaveOpen = leaveOpen)
            let pickler = resolver.Resolve elementType
            pickler.WriteSequence(writer, sequence)

        /// <summary>Lazily deserialize a sequence of objects from the underlying stream.</summary>
        /// <param name="stream">source stream.</param>
        /// <param name="length">number of elements to be deserialized.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        /// <returns>An IEnumerator that lazily consumes elements from the stream.</returns>
        member __.DeserializeSequence<'T>(stream : Stream, length : int, ?streamingContext : obj, ?encoding, ?leaveOpen) : IEnumerator<'T> =
            let reader = new Reader(stream, resolver, ?encoding = encoding, ?leaveOpen = leaveOpen)
            let pickler = resolver.Resolve<'T> ()
            reader.ReadSequence(pickler, length)

        /// <summary>Lazily deserialize a sequence of objects from the underlying stream.</summary>
        /// <param name="elementType">element type used in sequence.</param>
        /// <param name="stream">source stream.</param>
        /// <param name="length">number of elements to be deserialized.</param>
        /// <param name="streamingContext">untyped parameter passed to the streaming context.</param>
        /// <param name="encoding">encoding passed to the binary reader.</param>
        /// <param name="leaveOpen">leave underlying stream open when finished. Defaults to true.</param>
        /// <returns>An IEnumerator that lazily consumes elements from the stream.</returns>
        member __.DeserializeSequence(elementType : Type, stream : Stream, length : int, ?streamingContext : obj, ?encoding, ?leaveOpen) : IEnumerator =
            let reader = new Reader(stream, resolver, ?encoding = encoding, ?leaveOpen = leaveOpen)
            let pickler = resolver.Resolve elementType
            pickler.ReadSequence(reader, length)

        /// creates a byte array pickle out of given pickler and value
        member __.Pickle (pickler : Pickler<'T>) (value : 'T) : byte [] =
            use mem = new MemoryStream()
            __.Serialize(pickler, mem, value)
            mem.ToArray()

        /// unpickles a byte array using given pickler
        member __.UnPickle (pickler : Pickler<'T>) (data : byte []) =
            use mem = new MemoryStream(data)
            __.Deserialize(pickler, mem)


        /// Auto generates a pickler for given type variable
        member __.GeneratePickler<'T> () = resolver.Resolve<'T> ()
        
        /// Auto generates a pickler for given type
        member __.GeneratePickler (t : Type) = resolver.Resolve t


        /// Decides if given type is serializable by FsPickler
        member __.IsSerializableType (t : Type) =
            try resolver.Resolve t |> ignore ; true
            with :? NonSerializableTypeException -> false

        /// Decides if given type is serializable by FsPickler
        member __.IsSerializableType<'T> () =
            try resolver.Resolve<'T> () |> ignore ; true
            with :? NonSerializableTypeException -> false