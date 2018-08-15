open System.IO
open System.Reflection
open System.Diagnostics

// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

type Mode = 
    | Debug
    | Release
    | Bad

let parseDebugFlags (dflags: DebuggableAttribute.DebuggingModes) =
    let hasdefault = 
        (dflags &&& DebuggableAttribute.DebuggingModes.Default) = DebuggableAttribute.DebuggingModes.Default
    if hasdefault then Debug else Release

let readAsm f = 
    try 

        let a = Assembly.LoadFrom f
        let flags = a.GetCustomAttribute<DebuggableAttribute>().DebuggingFlags
        
        (parseDebugFlags flags, f)
    with
    | ex -> (Bad, f)
[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let d = @"."
    let files = Directory.GetFiles(d, "*.dll", SearchOption.AllDirectories)
    let all = 
        files
        |> Seq.map readAsm
        |> Seq.groupBy fst

    all
        |> Seq.iter (
            fun (g, files) ->
                printfn "%A" g
                files |> Seq.map snd |> Seq.iter (printfn "  %s"))
    match Seq.exists (fun (p,_) -> p = Debug) all with
    | true -> 2
    | false -> 0
           