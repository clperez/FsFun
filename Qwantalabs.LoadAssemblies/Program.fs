// Learn more about F# at http://fsharp.org

//https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext

open System
open System.Runtime.Loader

#if DEBUG
let assemblyPath = "C:\\projects\\FsFun\\Qwantalabs.DummyAssembly\\bin\\Debug\\netstandard2.0\\Qwantalabs.DummyAssembly.dll";
#else
let assemblyPath = "C:\\projects\\FsFun\\Qwantalabs.DummyAssembly\\bin\\Release\\netstandard2.0\\Qwantalabs.DummyAssembly.dll";
#endif

type MyAssemblyLoader() = 
    inherit AssemblyLoadContext(true)

let DoIt (loaderRef : WeakReference<AssemblyLoadContext> ) =
    try
        let success, loader = loaderRef.TryGetTarget()
        match success, loader with
        | false, _ -> Some("This is Broken")
        | true , loader ->  let assembly = loader.LoadFromAssemblyPath (assemblyPath)
                            let myType = assembly.GetType("Qwantalabs.DummyAssembly.Say", true) 
                            let method = myType.GetMethod("hello", Reflection.BindingFlags.Public ||| Reflection.BindingFlags.Static)
                            method.Invoke(null, [|"Carlos"|]) |> ignore
                            None
    with
        | _ -> Some("Something broke")


let rec CollectAndCheckRec (weakRef : WeakReference<AssemblyLoadContext>) counter =
    match weakRef.TryGetTarget() with
    | false, _      -> printfn "Counter: %d. DONE" counter
    | true,  _      -> match counter with 
                        | 0        ->   printfn "%A" "Number of Attempts depleted" 
                        | x as int ->   GC.Collect() 
                                        GC.WaitForPendingFinalizers() 
                                        printfn "Counter: %d" counter
                                        CollectAndCheckRec weakRef (counter - 1)

let CollectAndCheck weakRef =
    CollectAndCheckRec weakRef 10
                                
[<EntryPoint>]
let main argv =
    let myLoaderUploaded = upcast MyAssemblyLoader(): AssemblyLoadContext
    let testAlcWeakRef = new WeakReference<AssemblyLoadContext>(myLoaderUploaded);
    let result = DoIt testAlcWeakRef 
    myLoaderUploaded.Unload()
    myLoaderUploaded = null |> ignore;
    let xxx = CollectAndCheck testAlcWeakRef
    0

    
    
