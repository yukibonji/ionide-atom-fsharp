﻿namespace Atom.FSharp

open FunScript
open FunScript.TypeScript
open FunScript.TypeScript.fs
open FunScript.TypeScript.child_process
open FunScript.TypeScript.AtomCore
open FunScript.TypeScript.text_buffer

open Atom

[<AutoOpen>]
[<ReflectedDefinition>]
module ViewsHelpers =
    let jq(selector : string) = Globals.Dollar.Invoke selector
    let jq'(selector : Element) = Globals.Dollar.Invoke selector
    let jqC (context: Element) (selector : string) = Globals.Dollar.Invoke (selector,context)
    let (?) jq name = jq("#" + name)

    type cords = {
        mutable top : float
        mutable left : float
    }

    let getElementsByClass cls e =
        e
        |> fun n -> if JS.isDefined n then Some n else None
        |> Option.map( Atom.JS.getProperty<HTMLElement>("rootElement") )
        |> Option.map (fun n -> n.querySelectorAll(cls) )

    let pixelPositionFromMouseEvent (e : JQueryMouseEventObject) =
        getView
        >> getElementsByClass ".lines"
        >> Option.map( fun n -> n.[0] |> getBoundingClientRect)
        >> fun n' -> match n' with
                     | Some n -> { top = e.clientY - n.top; left =  e.clientX - n.left}
                     | None  -> {top = 0.; left = 0.}

    let screenPositionFromMouseEvent (e : JQueryMouseEventObject) (editor : IEditor) =
        editor.screenPositionForPixelPosition(pixelPositionFromMouseEvent e editor)


    let bufferPositionFromMouseEvent (e : JQueryMouseEventObject) (editor : IEditor) =
        pixelPositionFromMouseEvent e editor
        |> fun n -> let t = unbox<cords>(n) //TEMPORARY BUG FIX
                    t.top <- t.top + editor.displayBuffer.getScrollTop()
                    t.left <- t.left + editor.displayBuffer.getScrollLeft()
                    t
        |> editor.screenPositionForPixelPosition
        |> editor.bufferPositionForScreenPosition


[<ReflectedDefinition>]
module DTO =

    type Error = {
        /// 0-indexed first line of the error block
        StartLine : int
        /// 1-indexed first line of the error block
        StartLineAlternate : int
        /// 0-indexed first column of the error block
        StartColumn : int
        /// 1-indexed first column of the error block
        StartColumnAlternate : int
        /// 0-indexed last line of the error block
        EndLine : int
        /// 1-indexed last line of the error block
        EndLineAlternate : int
        /// 0-indexed last column of the error block
        EndColumn : int
        /// 1-indexed last column of the error block
        EndColumnAlternate : int
        /// Description of the error
        Message : string
        Severity : string
        /// Type of the Error
        Subcategory : string
        }

    type Declaration = {
        File : string
        Line : int
        Column : int
    }

    type Completion = {
        Name : string
        Glyph : string
        GlyphChar: string
    }

    type CompilerLocationResult = {Kind : string; Data : string}

    type CompletionResult = {Kind : string; Data : Completion []}
    type TooltipResult = {Kind : string; Data : string}
    type ParseResult = {Kind : string; Data : Error []}
    type FindDeclarationResult = {Kind : string; Data: Declaration}