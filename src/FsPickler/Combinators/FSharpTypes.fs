﻿namespace Nessos.FsPickler

    open System
    open System.IO

    // Pickler implementations for common F# generic types

    type internal OptionPickler =

        static member Create (ep : Pickler<'T>) =
            // Composite pickler filters None values by default
            let writer (w : WriteState) (tag : string) (x : 'T option) = ep.Write w "Some" x.Value

            let reader (r : ReadState) (tag : string) =
                let value = ep.Read r "Some"
                Some value

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)

        static member Create<'T> (resolver : IPicklerResolver) =
            let ep = resolver.Resolve<'T> ()
            OptionPickler.Create ep


    type internal ChoicePickler private () =

        static let c2 = UnionCaseSerializationHelper.OfUnionType<Choice<_,_>> ()
        static let c3 = UnionCaseSerializationHelper.OfUnionType<Choice<_,_,_>> ()
        static let c4 = UnionCaseSerializationHelper.OfUnionType<Choice<_,_,_,_>> ()
        static let c5 = UnionCaseSerializationHelper.OfUnionType<Choice<_,_,_,_,_>> ()
        static let c6 = UnionCaseSerializationHelper.OfUnionType<Choice<_,_,_,_,_,_>> ()
        static let c7 = UnionCaseSerializationHelper.OfUnionType<Choice<_,_,_,_,_,_,_>> ()

        static member Create(p1 : Pickler<'T1>, p2 : Pickler<'T2>) =

            let writer (w : WriteState) (tag : string) (c : Choice<'T1, 'T2>) =
                match c with
                | Choice1Of2 t1 -> 
                    c2.WriteTag(w.Formatter, 0)
                    p1.Write w "Item" t1
                | Choice2Of2 t2 -> 
                    c2.WriteTag(w.Formatter, 1)
                    p2.Write w "Item" t2

            let reader (r : ReadState) (tag : string) =
                match c2.ReadTag r.Formatter with
                | 0 -> p1.Read r "Item" |> Choice1Of2
                | 1 -> p2.Read r "Item" |> Choice2Of2
                | _ -> raise <| new FormatException("invalid choice branch.")

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)


        static member Create<'T1, 'T2> (resolver : IPicklerResolver) =
            let p1, p2 = resolver.Resolve<'T1> (), resolver.Resolve<'T2> ()
            ChoicePickler.Create(p1, p2)

        static member Create(p1 : Pickler<'T1>, p2 : Pickler<'T2>, p3 : Pickler<'T3>) =
            let writer (w : WriteState) (tag : string) (c : Choice<'T1, 'T2, 'T3>) =
                match c with
                | Choice1Of3 t1 -> 
                    c3.WriteTag(w.Formatter, 0)
                    p1.Write w "Item" t1
                | Choice2Of3 t2 -> 
                    c3.WriteTag(w.Formatter, 1)
                    p2.Write w "Item" t2
                | Choice3Of3 t3 -> 
                    c3.WriteTag(w.Formatter, 2)
                    p3.Write w "Item" t3

            let reader (r : ReadState) (tag : string) =
                match c3.ReadTag r.Formatter with
                | 0 -> p1.Read r "Item" |> Choice1Of3
                | 1 -> p2.Read r "Item" |> Choice2Of3
                | 2 -> p3.Read r "Item" |> Choice3Of3
                | _ -> raise <| new FormatException("invalid choice branch.")

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)


        static member Create<'T1, 'T2, 'T3> (resolver : IPicklerResolver) =
            let p1, p2, p3 = resolver.Resolve<'T1> (), resolver.Resolve<'T2> (), resolver.Resolve<'T3> ()
            ChoicePickler.Create(p1, p2, p3)

        static member Create(p1 : Pickler<'T1>, p2 : Pickler<'T2>, p3 : Pickler<'T3>, p4 : Pickler<'T4>) =
            let writer (w : WriteState) (tag : string) (c : Choice<'T1, 'T2, 'T3, 'T4>) =
                match c with
                | Choice1Of4 t1 -> 
                    c4.WriteTag(w.Formatter, 0)
                    p1.Write w "Item" t1
                | Choice2Of4 t2 -> 
                    c4.WriteTag(w.Formatter, 1)
                    p2.Write w "Item" t2
                | Choice3Of4 t3 -> 
                    c4.WriteTag(w.Formatter, 2)
                    p3.Write w "Item" t3
                | Choice4Of4 t4 -> 
                    c4.WriteTag(w.Formatter, 3)
                    p4.Write w "Item" t4

            let reader (r : ReadState) (tag : string) =
                match c4.ReadTag r.Formatter with
                | 0 -> p1.Read r "Item" |> Choice1Of4
                | 1 -> p2.Read r "Item" |> Choice2Of4
                | 2 -> p3.Read r "Item" |> Choice3Of4
                | 3 -> p4.Read r "Item" |> Choice4Of4
                | _ -> raise <| new FormatException("invalid choice branch.")

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)

        static member Create<'T1, 'T2, 'T3, 'T4> (resolver : IPicklerResolver) =
            let p1, p2 = resolver.Resolve<'T1> (), resolver.Resolve<'T2> ()
            let p3, p4 = resolver.Resolve<'T3> (), resolver.Resolve<'T4> ()
            ChoicePickler.Create(p1, p2, p3, p4)

        static member Create(p1 : Pickler<'T1>, p2 : Pickler<'T2>, p3 : Pickler<'T3>, 
                                p4 : Pickler<'T4>, p5 : Pickler<'T5>) =

            let writer (w : WriteState) (tag : string) (c : Choice<'T1, 'T2, 'T3, 'T4, 'T5>) =
                match c with
                | Choice1Of5 t1 -> 
                    c5.WriteTag(w.Formatter, 0)
                    p1.Write w "Item" t1
                | Choice2Of5 t2 -> 
                    c5.WriteTag(w.Formatter, 1)
                    p2.Write w "Item" t2
                | Choice3Of5 t3 -> 
                    c5.WriteTag(w.Formatter, 2)
                    p3.Write w "Item" t3
                | Choice4Of5 t4 -> 
                    c5.WriteTag(w.Formatter, 3)
                    p4.Write w "Item" t4
                | Choice5Of5 t5 ->
                    c5.WriteTag(w.Formatter, 4)
                    p5.Write w "Item" t5                  

            let reader (r : ReadState) (tag : string) =
                match c5.ReadTag r.Formatter with
                | 0 -> p1.Read r "Item" |> Choice1Of5
                | 1 -> p2.Read r "Item" |> Choice2Of5
                | 2 -> p3.Read r "Item" |> Choice3Of5
                | 3 -> p4.Read r "Item" |> Choice4Of5
                | 4 -> p5.Read r "Item" |> Choice5Of5
                | _ -> raise <| new FormatException("invalid choice branch.")

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)

        static member Create<'T1, 'T2, 'T3, 'T4, 'T5> (resolver : IPicklerResolver) =
            let p1, p2 = resolver.Resolve<'T1> (), resolver.Resolve<'T2> ()
            let p3, p4 = resolver.Resolve<'T3> (), resolver.Resolve<'T4> ()
            let p5     = resolver.Resolve<'T5> ()
            ChoicePickler.Create(p1, p2, p3, p4, p5)

        static member Create(p1 : Pickler<'T1>, p2 : Pickler<'T2>, p3 : Pickler<'T3>, 
                                p4 : Pickler<'T4>, p5 : Pickler<'T5>, p6 : Pickler<'T6>) =

            let writer (w : WriteState) (tag : string) (c : Choice<'T1, 'T2, 'T3, 'T4, 'T5, 'T6>) =
                match c with
                | Choice1Of6 t1 -> 
                    c6.WriteTag(w.Formatter, 0)
                    p1.Write w "Item" t1
                | Choice2Of6 t2 -> 
                    c6.WriteTag(w.Formatter, 1)
                    p2.Write w "Item" t2
                | Choice3Of6 t3 -> 
                    c6.WriteTag(w.Formatter, 2)
                    p3.Write w "Item" t3
                | Choice4Of6 t4 -> 
                    c6.WriteTag(w.Formatter, 3)
                    p4.Write w "Item" t4
                | Choice5Of6 t5 ->
                    c6.WriteTag(w.Formatter, 4)
                    p5.Write w "Item" t5
                | Choice6Of6 t6 ->
                    c6.WriteTag(w.Formatter, 5)
                    p6.Write w "Item" t6

            let reader (r : ReadState) (tag : string) =
                match c6.ReadTag r.Formatter with
                | 0 -> p1.Read r "Item" |> Choice1Of6
                | 1 -> p2.Read r "Item" |> Choice2Of6
                | 2 -> p3.Read r "Item" |> Choice3Of6
                | 3 -> p4.Read r "Item" |> Choice4Of6
                | 4 -> p5.Read r "Item" |> Choice5Of6
                | 5 -> p6.Read r "Item" |> Choice6Of6
                | _ -> raise <| new FormatException("invalid choice branch.")

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)

        static member Create<'T1, 'T2, 'T3, 'T4, 'T5, 'T6> (resolver : IPicklerResolver) =
            let p1, p2 = resolver.Resolve<'T1> (), resolver.Resolve<'T2> ()
            let p3, p4 = resolver.Resolve<'T3> (), resolver.Resolve<'T4> ()
            let p5, p6 = resolver.Resolve<'T5> (), resolver.Resolve<'T6> ()
            ChoicePickler.Create(p1, p2, p3, p4, p5, p6)

        static member Create(p1 : Pickler<'T1>, p2 : Pickler<'T2>, p3 : Pickler<'T3>, 
                                p4 : Pickler<'T4>, p5 : Pickler<'T5>, p6 : Pickler<'T6>, p7 : Pickler<'T7>) =

            let writer (w : WriteState) (tag : string) (c : Choice<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7>) =
                match c with
                | Choice1Of7 t1 -> 
                    c7.WriteTag(w.Formatter, 0)
                    p1.Write w "Item" t1
                | Choice2Of7 t2 -> 
                    c7.WriteTag(w.Formatter, 1)
                    p2.Write w "Item" t2
                | Choice3Of7 t3 -> 
                    c7.WriteTag(w.Formatter, 2)
                    p3.Write w "Item" t3
                | Choice4Of7 t4 -> 
                    c7.WriteTag(w.Formatter, 3)
                    p4.Write w "Item" t4
                | Choice5Of7 t5 ->
                    c7.WriteTag(w.Formatter, 4)
                    p5.Write w "Item" t5
                | Choice6Of7 t6 ->
                    c7.WriteTag(w.Formatter, 5)
                    p6.Write w "Item" t6
                | Choice7Of7 t7 ->
                    c7.WriteTag(w.Formatter, 6)
                    p7.Write w "Item" t7

            let reader (r : ReadState) (tag : string) =
                match c7.ReadTag r.Formatter with
                | 0 -> p1.Read r "Item" |> Choice1Of7
                | 1 -> p2.Read r "Item" |> Choice2Of7
                | 2 -> p3.Read r "Item" |> Choice3Of7
                | 3 -> p4.Read r "Item" |> Choice4Of7
                | 4 -> p5.Read r "Item" |> Choice5Of7
                | 5 -> p6.Read r "Item" |> Choice6Of7
                | 6 -> p7.Read r "Item" |> Choice7Of7
                | _ -> raise <| new FormatException("invalid choice branch.")

            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = true)

        static member Create<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7> (resolver : IPicklerResolver) =
            let p1, p2 = resolver.Resolve<'T1> (), resolver.Resolve<'T2> ()
            let p3, p4 = resolver.Resolve<'T3> (), resolver.Resolve<'T4> ()
            let p5, p6 = resolver.Resolve<'T5> (), resolver.Resolve<'T6> ()
            let p7     = resolver.Resolve<'T7> ()
            ChoicePickler.Create(p1, p2, p3, p4, p5, p6, p7)


    type internal FSharpRefPickler =
        static member Create (ep : Pickler<'T>) =
            let writer (w : WriteState) (tag : string) (r : 'T ref) =
                ep.Write w "contents" r.Value

            let reader (r : ReadState) (tag : string) =
                { contents = ep.Read r "contents" }

            // do not cache for performance
            CompositePickler.Create<_>(reader, writer, PicklerInfo.FSharpValue, cacheByRef = false, useWithSubtypes = false)
            
        static member Create<'T> (resolver : IPicklerResolver) =
            let ep = resolver.Resolve<'T> ()
            FSharpRefPickler.Create ep