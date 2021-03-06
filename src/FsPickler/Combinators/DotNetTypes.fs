﻿namespace Nessos.FsPickler

    open System
    open System.Reflection

    open Nessos.FsPickler

    /// abstract type pickler factory

    type internal AbstractPickler =
        static member Create<'T> () =
            let writer _ _ = invalidOp <| sprintf "Attempting to call abstract pickler '%O'." typeof<'T>
            let reader _ = invalidOp <| sprintf "Attempting to call abstract pickler '%O'." typeof<'T>

            CompositePickler.Create<'T>(reader, writer, PicklerInfo.FieldSerialization, true, false)

    /// Enum types combinator

    type internal EnumPickler =

        static member Create<'Enum, 'Underlying when 'Enum : enum<'Underlying>> (resolver : IPicklerResolver) =
            let pickler = resolver.Resolve<'Underlying> ()

            let writer (w : WriteState) (tag : string) (x : 'Enum) =
                let value = Microsoft.FSharp.Core.LanguagePrimitives.EnumToValue<'Enum, 'Underlying> x
                pickler.Write w "value" value

            let reader (r : ReadState) (tag : string) =
                let value = pickler.Read r "value"
                Microsoft.FSharp.Core.LanguagePrimitives.EnumOfValue<'Underlying, 'Enum> value

            CompositePickler.Create(reader, writer, PicklerInfo.FieldSerialization, cacheByRef = false, useWithSubtypes = false)


    /// Nullable Pickler combinator

    type internal NullablePickler =

        static member Create<'T when 
                                'T : (new : unit -> 'T) and 
                                'T : struct and 
                                'T :> ValueType> (resolver : IPicklerResolver) =

            let pickler = resolver.Resolve<'T> ()

            let writer (w : WriteState) (tag : string) (x : Nullable<'T>) =
                pickler.Write w "value" x.Value

            let reader (r : ReadState) (tag : string) =
                let value = pickler.Read r "value"
                Nullable<'T>(value)

            CompositePickler.Create(reader, writer, PicklerInfo.FieldSerialization, cacheByRef = false, useWithSubtypes = false)

    /// Delegate Pickler combinator

    type internal DelegatePickler =

        static member Create<'Delegate when 'Delegate :> Delegate> (resolver : IPicklerResolver) =
            let objPickler = resolver.Resolve<obj> ()
            let memberInfoPickler = resolver.Resolve<MethodInfo> ()
            let delePickler = resolver.Resolve<System.Delegate> ()

            let writer (w : WriteState) (tag : string) (dele : 'Delegate) =
                match dele.GetInvocationList() with
                | [| _ |] ->
                    w.Formatter.WriteBoolean "isLinked" false
                    memberInfoPickler.Write w "method" dele.Method
                    if not dele.Method.IsStatic then objPickler.Write w "target" dele.Target
                | deleList ->
                    w.Formatter.WriteBoolean "isLinked" true
                    w.Formatter.WriteInt32 "length" deleList.Length
                    for i = 0 to deleList.Length - 1 do
                        delePickler.Write w "linked" deleList.[i]

            let reader (r : ReadState) (tag : string) =
                if not <| r.Formatter.ReadBoolean "isLinked" then
                    let meth = memberInfoPickler.Read r "method"
                    if not meth.IsStatic then
                        let target = objPickler.Read r "target"
                        Delegate.CreateDelegate(typeof<'Delegate>, target, meth, throwOnBindFailure = true) |> fastUnbox<'Delegate>
                    else
                        Delegate.CreateDelegate(typeof<'Delegate>, meth, throwOnBindFailure = true) |> fastUnbox<'Delegate>
                else
                    let n = r.Formatter.ReadInt32 "length"
                    let deleList = Array.zeroCreate<System.Delegate> n
                    for i = 0 to n - 1 do deleList.[i] <- delePickler.Read r "linked"
                    Delegate.Combine deleList |> fastUnbox<'Delegate>

            CompositePickler.Create(reader, writer, PicklerInfo.Delegate, cacheByRef = true, useWithSubtypes = false)