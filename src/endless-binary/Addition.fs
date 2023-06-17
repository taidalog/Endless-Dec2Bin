// taidalab Version 4.2.0
// https://github.com/taidalog/taidalab
// Copyright (c) 2022-2023 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/taidalab/blob/main/LICENSE
namespace Taidalab

open System
open Browser.Dom
open Browser.Types
open Taidalab.Number
open Taidalab.Text
open Taidalab.EndlessBinary
open Fermata
open Fermata.RadixConversion

module EndlessBinary =
    module Addition =
        let help = """
            2進数同士の足し算をエンドレスで練習できます。<br>
            出題範囲は m, n (2 &le; m + n &le; 255) で、繰り上がりもあります。<br>
            ヒント付きなので、考え方も身に付けられます。
            """

        let newHintAdd ()  =
            let hint = """
                <details><summary>ヒント: </summary>
                    <p class="history-indented">
                        10進数の筆算と同じように、右端から上下の数を足していきます。<br><br>
                        0<sub>(2)</sub> + 0<sub>(2)</sub> = 0<sub>(2)</sub><br>
                        0<sub>(2)</sub> + 1<sub>(2)</sub> = 1<sub>(2)</sub><br>
                        1<sub>(2)</sub> + 1<sub>(2)</sub> = 10<sub>(2)</sub><br>
                        1<sub>(2)</sub> + 1<sub>(2)</sub> + 1<sub>(2)</sub> = 11<sub>(2)</sub><br><br>
                        10<sub>(2)</sub> や 11<sub>(2)</sub>のように桁が繰り上がった時は、<br>
                        繰り上がった桁 (=1) をひとつ左の桁に足します。<br>
                    </p>
                </details>"""
            hint
        
        let newNumbersAdd digit =
            let max = (digit |> float |> fun x -> 2.0 ** x |> int) - 1
            let number1 =
                newNumber
                    (fun _ -> getRandomBetween 1 max)
                    (if digit < 4 then
                        (fun n -> n |> Dec.toBin |> String.length = digit)
                    else
                        (fun n ->
                            let pattern = "^1+0+$"
                            let bin = Dec.toBin n
                            (String.length bin = digit) && (Regex.isMatch pattern bin) = false))
            let number2 =
                let max' = if digit < 4 then max else max - number1
                newNumber
                    (fun _ -> getRandomBetween 1 max')
                    (if digit < 4
                    then (fun _ -> true)
                    else (fun n -> (n <> number1) && ((n &&& number1) <> 0)))
            (number1, number2)
        
        let question digit lastAnswers : int * int =
            newNumber
                (fun _ -> newNumbersAdd digit)
                (if digit < 4
                then (fun _ -> true)
                else (fun (x, y) -> List.contains x lastAnswers = false && List.contains y lastAnswers = false))

        let rec checkAnswer (questionGenerator: 'c list -> 'd) answer num1 num2 (last_answers : int list) =
            // Getting the user input.
            let numberInput = document.getElementById "numberInput" :?> HTMLInputElement
            let input = numberInput.value |> escapeHtml
            let bin: Result<string,Errors.Errors> = input |> Bin.validate
            //printfn "bin: %s" bin
            
            numberInput.focus()

            let sourceRadix = 2
            
            match bin with
            | Error (error: Errors.Errors) ->
                // Making an error message.
                newErrorMessageBin
                    $"%s{Dec.toBin num1}<sub>(%d{sourceRadix})</sub> + %s{Dec.toBin num2}<sub>(%d{sourceRadix})</sub>"
                    input
                    error
                |> fun x -> (document.getElementById "errorArea").innerHTML <- x
            | Ok (bin: string) ->
                (document.getElementById "errorArea").innerHTML <- ""
                // Converting the input in order to use in the history message.
                let binaryDigit = 8
                let taggedBin = bin |> padWithZero binaryDigit |> colorLeadingZero 
                //printfn "taggedBin: %s" taggedBin
                
                let decDigit = 3
                let dec = Bin.toDec bin
                let spacePaddedDec = dec |> string |> Fermata.String.padLeft decDigit ' ' |> escapeSpace
                //printfn "dec: %d" dec
                //printfn "spacePaddedInputValue: %s" spacePaddedDec
                
                // Making a new history and updating the history with the new one.
                let destinationRadix = 10
                let outputArea = document.getElementById "outputArea"
                let historyMessage =
                    newHistory(dec = answer) taggedBin sourceRadix spacePaddedDec destinationRadix
                    |> (fun x -> concatinateStrings "<br>" [x; outputArea.innerHTML])
                //printfn "historyMessage: `n%s" historyMessage
                outputArea.innerHTML <- historyMessage
                
                if dec = answer then
                    // Making the next question.
                    let (number1, number2) = questionGenerator last_answers
                        //newNumber
                        //    (fun _ -> newNumbersAdd 4)
                        //    (fun (n1, n2) ->
                        //        List.contains n1 last_answers = false && List.contains n2 last_answers = false)

                    //printfn "%A" last_answers
                    //printfn "number1: %d" number1
                    //printfn "number1 |> Dec.toBin: %s" (number1 |> Dec.toBin)
                    //printfn "number2: %d" number2
                    //printfn "number2 |> Dec.toBin: %s" (number2 |> Dec.toBin)
                    //printfn "number1 + number2: %d" (number1 + number2)
                    //printfn "number1 + number2 |> Dec.toBin: %s" (number1 + number2 |> Dec.toBin)
                    setColumnAddition number1 number2

                    let nextHint = newHintAdd ()
                    (document.getElementById "hintArea").innerHTML <-  nextHint
                    //printfn "nextHint: `n%s" nextHint

                    numberInput.value <- ""

                    // Updating `lastAnswers`.
                    // These numbers will not be used for the next question.
                    let answersToKeep = Math.Min(20, List.length last_answers + 1)
                    let lastAnswers = ([number1; number2] @ last_answers).[0..(answersToKeep - 1)]

                    // Setting the next answer to the check button.
                    (document.getElementById "submitButton").onclick <- (fun _ ->
                        checkAnswer questionGenerator (number1 + number2) number1 number2 lastAnswers
                        false)
                    (document.getElementById "inputArea").onsubmit <- (fun _ ->
                        checkAnswer questionGenerator (number1 + number2) number1 number2 lastAnswers
                        false)


        let init  () =
            // Initialization.
            let sourceRadix = 2
            let destinationRadix = 2
            let hint = newHintAdd ()
            let digit = 2

            (document.getElementById "numberInput").className <-  "number-input question-number eight-digit"
            (document.getElementById "operator").innerText <-  "+)"
            (document.getElementById "firstRowSrcRadix").innerText <- sprintf "(%d)" sourceRadix
            (document.getElementById "secondRowSrcRadix").innerText <-  sprintf "(%d)" sourceRadix
            (document.getElementById "binaryRadix").innerHTML <-  sprintf "<sub>(%d)</sub>" destinationRadix
            (document.getElementById "hintArea").innerHTML <-  hint

            let (number1, number2) = question digit [] //newNumbersAdd digit
            //printfn "number1: %d" number1
            //printfn "number1 |> Dec.toBin: %s" (number1 |> Dec.toBin)
            //printfn "number2: %d" number2
            //printfn "number2 |> Dec.toBin: %s" (number2 |> Dec.toBin)
            //printfn "number1 + number2: %d" (number1 + number2)
            //printfn "number1 + number2 |> Dec.toBin: %s" (number1 + number2 |> Dec.toBin)
            setColumnAddition number1 number2

            (document.getElementById "submitButton").onclick <- (fun _ ->
                checkAnswer (question digit) (number1 + number2) number1 number2 [number1; number2]
                false)
            (document.getElementById "inputArea").onsubmit <- (fun _ ->
                checkAnswer (question digit) (number1 + number2) number1 number2 [number1; number2]
                false)
            
            (document.getElementById "helpButton").onclick <- (fun _ ->
                ["helpWindow"; "helpBarrier"]
                |> List.iter (fun x -> (document.getElementById x).classList.toggle "active" |> ignore))
            
            (document.getElementById "helpBarrier").onclick <- (fun _ ->
                ["helpWindow"; "helpBarrier"]
                |> List.iter (fun x -> (document.getElementById x).classList.remove "active" |> ignore))
