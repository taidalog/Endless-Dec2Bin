// taidalab
// https://github.com/taidalog/taidalab
// Copyright (c) 2022-2025 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/taidalab/blob/main/LICENSE
namespace Taidalab

open System
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fermata
open Fermata.RadixConversion

module EndlessBinary =
    module Home =
        let main =
            """
            <form class="button-container">
                <button type="button" id="buttonED2B1" class="btn course-button-d2b1 display-order-1">10進数→2進数 (1)</button>
                <button type="button" id="buttonED2B2" class="btn course-button-d2b2 display-order-2">10進数→2進数 (2)</button>
                <button type="button" id="buttonEB2D1" class="btn course-button-b2d1 display-order-3">2進数→10進数 (1)</button>
                <button type="button" id="buttonEB2D2" class="btn course-button-b2d2 display-order-4">2進数→10進数 (2)</button>
                <button type="button" id="buttonEPOT1" class="btn course-button-pot1 display-order-5">2のn乗</button>
                <button type="button" id="buttonEPOT2" class="btn course-button-pot2 display-order-6">2のn乗-1</button>
                <button type="button" id="buttonEBAD" class="btn course-button-add display-order-7">加算</button>
                <button type="button" id="buttonEBSB" class="btn course-button-sub display-order-8">減算</button>
                <button type="button" id="buttonECMP" class="btn course-button-cmp display-order-9">補数</button>
                <button type="button" id="buttonED2H" class="btn course-button-d2h display-order-10">10進数→16進数</button>
                <button type="button" id="buttonEH2D" class="btn course-button-d2h display-order-11">16進数→10進数</button>
            </form>"""

    module Course =
        let main help colorClass =
            $"""
            <span id="questionArea" class="question-area"></span>
            <form id="inputArea" class="input-area" autocomplete="off">
                <input type="text" id="numberInput" class="number-input display-order-1 mono regular">
                <span id="binaryRadix" class="binary-radix display-order-2"></span>
                <button type="button" id="submitButton" class="submit-button display-order-3 d2b-button">確認</button>
                <div id="errorArea" class="error-area display-order-4"></div>
                <div id="hintArea" class="hint-area display-order-5"></div>
            </form>
            <div class="history-area">
                <h2>結果:</h2>
                <div class="history-indented mono regular">
                    <span id="outputArea"></span>
                </div>
            </div>
            <div id="helpWindow" class="help-window">
                <div class="help-close-outer">
                    <span id="helpClose" class="material-symbols-outlined help-close %s{colorClass}" translate="no">
                        close
                    </span>
                </div>
                %s{help}
            </div>"""

        let main404 =
            """
            <span id="questionArea" class="question-area"></span>
            <form id="inputArea" class="input-area" autocomplete="off">
                <input type="text" id="numberInput" class="number-input display-order-1 mono regular">
                <span id="binaryRadix" class="binary-radix display-order-2"></span>
                <button type="button" id="submitButton" class="submit-button display-order-3 d2b-button">確認</button>
                <div id="errorArea" class="error-area display-order-4"></div>
                <div id="hintArea" class="hint-area display-order-5"></div>
            </form>
            <div class="history-area">
                <h2>結果:</h2>
                <div class="history-indented mono regular">
                    <span id="outputArea"></span>
                </div>
            </div>"""

    let newErrorMessageBin (question: string) (input: string) (error: exn) =
        if String.IsNullOrWhiteSpace input then
            $"%s{question} の2進法表記を入力してください。"
        else if Regex.isMatch "^[01]+$" input |> not then
            $"'%s{input}' は2進数ではありません。使えるのは半角の 0 と 1 のみです。"
        // else if false then
        //      $"'%s{input}' は入力できる数値の範囲を越えています。入力できるのは xxx ~ yyy の間です。"
        else
            "不明なエラーです。"
        |> sprintf """<span class="warning">%s</span>"""

    let newErrorMessageDec (question: string) (input: string) (error: exn) =
        if String.IsNullOrWhiteSpace input then
            $"%s{question} の10進法表記を入力してください。"
        else if Regex.isMatch "^[0-9]+$" input |> not then
            $"'%s{input}' は10進数ではありません。使えるのは半角の 0123456789 のみです。"
        // else if false then
        //      $"'%s{input}' は入力できる数値の範囲を越えています。入力できるのは xxx ~ yyy の間です。"
        else
            "不明なエラーです。"
        |> sprintf """<span class="warning">%s</span>"""

    let newErrorMessageHex (question: string) (input: string) (error: exn) =
        if String.IsNullOrWhiteSpace input then
            $"%s{question} の16進法表記を入力してください。"
        else if Regex.isMatch "^[0-9A-Fa-f]+$" input |> not then
            $"'%s{input}' は16進数ではありません。使えるのは半角の 0123456789ABCDEF のみです。"
        // else if false then
        //      $"'%s{input}' は入力できる数値の範囲を越えています。入力できるのは xxx ~ yyy の間です。"
        else
            "不明なエラーです。"
        |> sprintf """<span class="warning">%s</span>"""

    let newHistory correct input destination_radix converted_input source_radix =
        let historyClassName =
            if correct then
                "history history-correct"
            else
                "history history-wrong"

        let historyIcon =
            if correct then
                """<span class="material-symbols-outlined history-correct" translate="no">check_circle</span>"""
            else
                """<span class="material-symbols-outlined history-wrong" translate="no">error</span>"""

        $"""
        <div class="history-container %s{historyClassName}"">
            %s{historyIcon}<span class ="%s{historyClassName}">%s{input}<sub>(%d{destination_radix})</sub> = %s{converted_input}<sub>(%d{source_radix})</sub></span>
        </div>
        """

    let splitBinaryStringBy digit bin =
        match bin with
        | Bin.Invalid _ -> ""
        | Bin.Valid v -> v |> String.chunkBySizeRight digit |> Seq.toList |> String.concat " "

    open Browser.Dom

    let setColumnAddition (number1: int) (number2: int) =
        let bin1 =
            number1
            |> Dec.Valid
            |> Dec.toBin
            |> function
                | Bin.Valid v -> v
                | Bin.Invalid _ -> ""
            |> Seq.map string
            |> Seq.padLeft 8 ""

        let bin2 =
            number2
            |> Dec.Valid
            |> Dec.toBin
            |> function
                | Bin.Valid v -> v
                | Bin.Invalid _ -> ""
            |> Seq.map string
            |> Seq.padLeft 8 ""

        bin1
        |> Seq.iteri (fun i x ->
            $"firstRowDigit%d{8 - i}"
            |> document.getElementById
            |> fun elm -> elm.innerText <- x)

        bin2
        |> Seq.iteri (fun i x ->
            $"secondRowDigit%d{8 - i}"
            |> document.getElementById
            |> fun elm -> elm.innerText <- x)

    let delayMs index = index * 2500 - 500 |> abs

    let numOpt radix num = (Some radix, Some 1, Some num, None)

    let divRemOpt divisor divRem =
        match divRem |> List.rev with
        | [] -> [ (None, None, None, None) ]
        | h :: t ->
            let inner_h = h |> (fun (x, y) -> (None, None, Some x, Some y))
            let inner_t = t |> List.map (fun (x, y) -> (Some divisor, Some 1, Some x, Some y))
            inner_h :: inner_t |> List.rev

    let keyboardshortcut (e: KeyboardEvent) =
        match document.activeElement.id with
        | "numberInput" ->
            match e.key with
            | "Escape" -> (document.getElementById "numberInput").blur ()
            | _ -> ()
        | _ ->
            let isHelpWindowActive =
                (document.getElementById "helpWindow").classList
                |> (fun x -> JS.Constructors.Array?from(x))
                |> Array.contains "active"

            match e.key with
            | "\\" ->
                if not isHelpWindowActive then
                    (document.getElementById "numberInput").focus ()
                    e.preventDefault ()
            | "?" ->
                [ "helpWindow"; "helpBarrier" ]
                |> List.iter (fun x -> (document.getElementById x).classList.toggle "active" |> ignore)
            | "Escape" ->

                if isHelpWindowActive then
                    [ "helpWindow"; "helpBarrier" ]
                    |> List.iter (fun x -> (document.getElementById x).classList.remove "active" |> ignore)
            | _ -> ()
