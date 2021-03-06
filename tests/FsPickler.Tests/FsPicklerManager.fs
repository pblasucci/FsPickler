﻿namespace Nessos.FsPickler.Tests

    open System

    open Nessos.FsPickler
    open Nessos.FsPickler.Json

    [<RequireQualifiedAccess>]
    module PickleFormat =
        [<Literal>]
        let Binary = "FsPickler.Binary"

        [<Literal>]
        let Xml = "FsPickler.Xml"

        [<Literal>]
        let Json = "FsPickler.Json"

        [<Literal>]
        let Json_Headerless = "FsPickler.Json-headerless"

        [<Literal>]
        let Bson = "FsPickler.Bson"

    type FsPicklerManager(pickleFormat : string) =

        let pickler =
            match pickleFormat with
            | PickleFormat.Binary -> FsPickler.CreateBinary() :> FsPicklerSerializer
            | PickleFormat.Xml -> FsPickler.CreateXml(indent = true) :> FsPicklerSerializer
            | PickleFormat.Json -> FsPickler.CreateJson(indent = true) :> FsPicklerSerializer
            | PickleFormat.Json_Headerless -> 
                let jsp = FsPickler.CreateJson(omitHeader = true)
                jsp.UseCustomTopLevelSequenceSeparator <- true
                jsp.SequenceSeparator <- System.Environment.NewLine
                jsp :> FsPicklerSerializer

            | PickleFormat.Bson -> FsPickler.CreateBson() :> FsPicklerSerializer

            | _ -> invalidArg "name" <| sprintf "unexpected pickler format '%s'." pickleFormat

        member __.Pickler = pickler

        member __.GetRemoteSerializer() = new RemoteSerializationClient(pickleFormat)

        interface IPickler with
            member __.Name = pickleFormat
            member __.Pickle (value : 'T) = pickler.Pickle(value)
            member __.UnPickle<'T> (data : byte[]) = pickler.UnPickle<'T>(data)



    and RemoteSerializer (pickleFormat : string) =
        inherit MarshalByRefObject()

        let mgr = new FsPicklerManager(pickleFormat)
        let fp = FailoverPickler.Create()

        member __.Pickle<'T>(data : byte []) : byte [] =
            let value = fp.UnPickle<'T>(data)
            mgr.Pickler.Pickle<'T>(value)

        member __.PickleF(data : byte []) : byte [] =
            let serializer = fp.UnPickle<FsPicklerSerializer -> byte[]>(data)
            serializer mgr.Pickler

    and RemoteSerializationClient (pickleFormat : string) =
        let fp = FailoverPickler.Create()
        let remote = AppDomainManager.Activate<RemoteSerializer>("remoteSerializationDomain", [| pickleFormat :> obj |])

        member __.Pickle<'T> (value : 'T) =
            let data = fp.Pickle<'T> value
            remote.Pickle<'T> data

        member __.PickleF (pickleF : FsPicklerSerializer -> byte []) =
            let data = fp.Pickle pickleF
            remote.PickleF data