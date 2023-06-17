// taidalab Version 4.3.0
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
    module Dec2Bin1 =
        let help = """
            10進数から2進数への変換をエンドレスで練習できます。<br>
            出題範囲は n (0&le;n&le;255) で、2の累乗の数同士の和になっています。<br>
            ヒント付きなので、考え方も身に付けられます。
            """
        
//        let countOneBit binaryString =
//            binaryString
//            |> Seq.countWith (fun c -> c = '1')


        let devideIntoPowerOfTwo (number : int) =
            let getMaxPowerOfTwo (number : int) =
                let indexNumber = Math.Log(double number, 2.0) |> int |> double
                Math.Pow(2.0, indexNumber) |> int

            let rec loop acc number =
                match number with
                | 0 -> acc
                | 1 -> acc @ [1]
                | _ ->
                    let max = getMaxPowerOfTwo number
                    loop (acc @ [max]) (number - max)

            loop [] number


        let rec repeatDivision dividend divisor =
            let quotient = int (dividend / divisor)
            let remainder = dividend - (quotient * divisor)
            if quotient < divisor then
                [(quotient, remainder)]
            else
                [(quotient, remainder)] @ repeatDivision quotient divisor


        let newNumberWithTwoOne () =
            let rec newTwoRandomNumbers min max =
                let rand = new Random()
                let index1 = rand.Next(min, max)
                let index2 = rand.Next(min, max)
                if index1 <> index2 then
                    (index1, index2)
                else
                    newTwoRandomNumbers min max
            newTwoRandomNumbers 0 7
            ||> (fun x y -> double x, double y)
            ||> (fun x y -> Math.Pow(2.0, x) + Math.Pow(2.0, y))
            |> int
        

        let newArrowBin fontSize lineCount stroke fill=
            Svg.newArrow
                (fontSize |> double |> (fun x -> x / 2. * 4.))
                (lineCount |> (fun x -> (fontSize * (x - 1)) + 6) |> double)
                (fontSize |> double |> (fun x -> x / 2. * 3.))
                (lineCount |> double |> (fun x -> 17.85 * x - 35.) |> ((*) -1.))
                -48.
                (17.85 * (lineCount |> double) - 15.)
                (lineCount - 1 |> delayMs |> ((+) 1500))
                stroke
                fill

        
        let newHintAnimation divisor num fontSize =
            let divRems =
                (numOpt divisor num) :: (divRemOpt divisor (repeatDivision num divisor))
            divRems
            |> List.mapi (fun i (a, b, c, d) ->
                Option.map // divisor
                    (fun x ->
                        Svg.text
                            0
                            (fontSize * (i + 1))
                            0.
                            (sprintf "%d%s" x (Svg.animateOpacity (i |> delayMs |> (fun x -> if i = 0 then x + 1000 else x + 2000)) 500)))
                    a,
                Option.map // line
                    (fun x ->
                        Svg.path
                            (sprintf
                                "M %d,%d q %d,%f 0,%f h %f"
                                (fontSize / 2 + 2)
                                ((fontSize * i) + 6)
                                (fontSize / 2)
                                (double fontSize * 0.4)
                                (double fontSize * 0.8)
                                (double fontSize / 2.* 4.8))
                            "#000000"
                            1
                            "none"
                            0.
                            (Svg.animateOpacity (i |> delayMs |> (fun x -> if i = 0 then x + 500 else x + 1500)) 500))
                    b,
                Option.map // dividend
                    (fun x ->
                        Svg.text
                            (fontSize / 2 * 2)
                            (fontSize * (i + 1))
                            0.
                            (sprintf "%s%s" (x |> string |> (Fermata.String.padLeft 3 ' ') |> escapeSpace) (Svg.animateOpacity (i |> delayMs) 500)))
                    c,
                Option.map // remainder
                    (fun x ->
                        Svg.text
                            (fontSize / 2 * 6)
                            (fontSize * (i + 1))
                            0.
                            (sprintf "…%d%s" x (Svg.animateOpacity (i |> delayMs |> ((+) 500)) 500)))
                    d)
            |> List.map (fun (a, b, c, d) ->
                sprintf
                    "%s%s%s%s"
                    (Option.defaultValue "" a)
                    (Option.defaultValue "" b)
                    (Option.defaultValue "" c)
                    (Option.defaultValue "" d))
            |> List.fold
                (fun x y -> sprintf "%s%s" x y)
                (newArrowBin fontSize (List.length divRems) "#191970" "#b0e0e6")
            |> (Svg.frame
                    (fontSize / 2 * 10)
                    (divRems |> List.length |> (fun x -> fontSize * (x + 1))))
        
//        let hint content=
//            sprintf """<details id="hintDetails"><summary>ヒント: </summary>%s</details>""" content

        let newHintRepeatDivision divisor number =
            sprintf
                """
                <div class="history-indented">
                    <p>
                        10進法で表現した数を2進法で表現しなおすには、<br>
                        10進法の数を、商が 1 になるまで 2 で割り続けます。<br>
                        この時、余りを商の右に書いておきます。<br>
                        商と余りを下から順に繋げると、2進法の数になります。<br>
                        ※この下の筆算をクリックすると動きます。
                    </p>
                </div>
                <div id="hint1" class="history-indented column-addition-area">
                    %s
                </div>"""
                (newHintAnimation divisor number 20)

        let newHintRepeatAddition number (power_of_twos : int list) =
            let additionDec = power_of_twos |> List.map string |> String.concat " + "
            let log2 i = Math.Log(double i, 2.0)
            let additionIndex =
                power_of_twos
                |> List.map (log2 >> Math.Truncate >> int)
                |> List.map (sprintf "2<sup>%d</sup>")
                |> String.concat " + "
            let additionBin =
                power_of_twos
                |> List.map Dec.toBin
                |> List.map (sprintf "%s<sub>(2)</sub>")
                |> String.concat " + "
            $"""
                <p class="history-indented">
                    10進法で表現した数を2進法で表現しなおすには、<br>
                </p>
                <p class="history-indented">
                    <ol style="padding-left: 4rem;">
                        <li>10進法の数を「2<sup>n</sup> の数同士の足し算」に変換して、</li>
                        <li>それぞれの 2<sup>n</sup> の数を2進法で表し、</li>
                        <li>足し合わせる</li>
                    </ol>
                </p>
                <p class="history-indented">
                    という方法もあります。
                </p>
                <p class="history-indented">
                    %d{number}<sub>(10)</sub> を 2<sup>n</sup> の数同士の足し算に変換すると
                </p>
                <p class="history-indented hint-bgcolor-gray">
                    &nbsp;&nbsp;%s{additionDec}<br>
                    = %s{additionIndex}
                </p>
                <p class="history-indented">
                    になります。<br>
                </p>
                <p class="history-indented">
                    次に、それぞれの 2<sup>n</sup> の数を2進法で表します。<br>
                    2<sup>n</sup> の数を2進法で表すには、1 の後に 0 を n 個続けます。<br>
                    そのため、%s{additionIndex} は2進法で
                </p>
                <p class="history-indented hint-bgcolor-gray">
                    &nbsp;&nbsp;%s{additionBin}<br>
                </p>
                <p class="history-indented">
                    と表現できます。最後にこれを計算すると
                </p>
                <p class="history-indented hint-bgcolor-gray">
                    &nbsp;&nbsp;%s{additionBin}<br>
                    = %s{number |> Dec.toBin}<sub>(2)</sub>
                </p>
                <p class="history-indented">
                    になります。
                </p>"""

        let newHint divisor number power_of_twos =
            sprintf
                """
                <details id="hintDetails">
                    <summary>ヒント: </summary>
                    <h2>考え方 1</h2>
                    %s
                    <h2>考え方 2</h2>
                    %s
                </details>
                """
                (newHintRepeatDivision divisor number)
                (newHintRepeatAddition number power_of_twos)

        let hint number =
            newHint 2 number (devideIntoPowerOfTwo number)
        
        let question lastAnswers : int =
            newNumber
                (fun _ -> newNumberWithTwoOne ())
                (fun n -> List.contains n lastAnswers = false)

        let additional number : unit =
            (document.getElementById "hint1").onclick <- (fun _ ->
                (document.getElementById "hint1").innerHTML <-
                    newHintAnimation 2 number 20
                (document.getElementById "hintDetails").setAttribute ("open", "true"))
        
        let rec checkAnswer (questionGenerator: 'c list -> 'c) (hintGenerator: 'a -> 'b) validator converter tagger (additional: 'c -> unit) sourceRadix destinationRadix (answersToKeep: int) (answer: string) (last_answers : int list) =
            // Getting the user input.
            let numberInput = document.getElementById "numberInput" :?> HTMLInputElement
            let input = numberInput.value |> escapeHtml
            let validated: Result<string,Errors.Errors> = input |> validator
            //printfn "bin: %A" bin
            
            numberInput.focus()
            
            match validated with
            | Error (error: Errors.Errors) ->
                // Making an error message.
                (document.getElementById "errorArea").innerHTML <- newErrorMessageBin answer input error
            | Ok validated ->
                (document.getElementById "errorArea").innerHTML <- ""

                // Converting the input in order to use in the history message.
                //let binaryDigit = 8
                //let destinationRadix = 2
                let colored = validated |> tagger //padWithZero binaryDigit |> colorLeadingZero
                let converted = validated |> converter
                //printfn "taggedBin: %s" taggedBin
                //printfn "dec: %d" dec
                
                let decimalDigit = 3
                let spacePadded =
                    converted
                    |> string
                    |> Fermata.String.padLeft decimalDigit ' '
                    |> escapeSpace
                
                // Making a new history and updating the history with the new one.
                //let sourceRadix = 10
                let outputArea = document.getElementById "outputArea" :?> HTMLParagraphElement
                let historyMessage =
                    newHistory (converted = int answer) colored destinationRadix spacePadded sourceRadix
                    |> (fun x -> concatinateStrings "<br>" [x; outputArea.innerHTML])
                //printfn "historyMessage: \n%s" historyMessage
                outputArea.innerHTML <- historyMessage
                
                if converted <> int answer then
                    ()
                else
                    // Making the next question.
                    //printfn "last_answers : %A" last_answers
                    
                    let nextNumber = questionGenerator last_answers
                    //printfn "nextNumber : %d" nextNumber
                    //printfn "List.contains nextNumber last_answers : %b" (List.contains nextNumber last_answers)

                    //let quotientsAndRemainders = repeatDivision nextNumber 2
                    //printfn "quotientsAndRemainders: %A" quotientsAndRemainders
                    
                    //let powerOfTwos = devideIntoPowerOfTwo nextNumber
                    //printfn "powerOfTwos: %A" powerOfTwos

                    //let nextHint = hint nextNumber
                    //printfn "nextHint: \n%s" nextHint
                    
                    (document.getElementById "questionSpan").innerText <- string nextNumber
                    (document.getElementById "hintArea").innerHTML <- hintGenerator nextNumber
                    additional nextNumber
                    
                    numberInput.value <- ""

                    // Updating `lastAnswers`.
                    // These numbers will not be used for the next question.
                    //let answersToKeep = Math.Min(10, List.length last_answers + 1)
                    //let lastAnswers = (nextNumber :: last_answers).[0..(answersToKeep - 1)]
                    let lastAnswers' = (nextNumber :: last_answers) |> List.truncate answersToKeep

                    // Setting the next answer to the check button.
                    (document.getElementById "submitButton").onclick <- (fun _ ->
                        checkAnswer questionGenerator hintGenerator validator converter tagger additional sourceRadix destinationRadix answersToKeep (string nextNumber) lastAnswers'
                        false)
                    (document.getElementById "inputArea").onsubmit <- (fun _ ->
                        checkAnswer questionGenerator hintGenerator validator converter tagger additional sourceRadix destinationRadix answersToKeep (string nextNumber) lastAnswers'
                        false)


        let init' (questionGenerator: 'c list -> 'c) (hintGenerator: 'a -> 'b) validator converter tagger (additional: 'c -> unit) sourceRadix destinationRadix (answersToKeep: int) checker : unit =
            // Initialization.
            //printfn "Initialization starts."

            let initNumber = questionGenerator []
            //printfn "initNumber : %d" initNumber

            //let quotientsAndRemainders = repeatDivision initNumber 2
            //let powerOfTwos = devideIntoPowerOfTwo initNumber
            //printfn "quotients and remainders : %A" quotientsAndRemainders
            //printfn "power of twos : %A" powerOfTwos

            //let sourceRadix = 10
            //let destinationRadix = 2

            (document.getElementById "questionSpan").innerText <- string initNumber
            (document.getElementById "srcRadix").innerText <- sprintf "(%d)" sourceRadix
            (document.getElementById "dstRadix").innerText <- string destinationRadix
            (document.getElementById "binaryRadix").innerHTML <- sprintf "<sub>(%d)</sub>" destinationRadix
            (document.getElementById "hintArea").innerHTML <- hintGenerator initNumber
            (document.getElementById "submitButton").onclick <- (fun _ ->
                checker questionGenerator hintGenerator validator converter tagger additional sourceRadix destinationRadix answersToKeep (string initNumber) [initNumber]
                false)
            (document.getElementById "inputArea").onsubmit <- (fun _ ->
                checker questionGenerator hintGenerator validator converter tagger additional sourceRadix destinationRadix answersToKeep (string initNumber) [initNumber]
                false)
            additional initNumber
            
            (document.getElementById "helpButton").onclick <- (fun _ ->
                ["helpWindow"; "helpBarrier"]
                |> List.iter (fun x -> (document.getElementById x).classList.toggle "active" |> ignore))
            
            (document.getElementById "helpBarrier").onclick <- (fun _ ->
                ["helpWindow"; "helpBarrier"]
                |> List.iter (fun x -> (document.getElementById x).classList.remove "active" |> ignore))
            
            //printfn "Initialization ends."
        
        let init () = init' question hint Bin.validate Bin.toDec (padWithZero 8 >> colorLeadingZero) additional 10 2 10 checkAnswer
