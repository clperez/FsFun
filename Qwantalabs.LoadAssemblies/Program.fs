// Learn more about F# at http://fsharp.org

open System
open System.Runtime.Loader

type MyAssemblyLoader() = 
    inherit AssemblyLoadContext(true)


[<EntryPoint>]
let main argv =
    let myloader = MyAssemblyLoader()
    printfn "Loading an assembly"
    
    let assembly = myloader.LoadFromAssemblyPath ("C:\\projects\\FsFun\\Qwantalabs.DummyAssembly\\Qwantalabs.DummyAssembly.dll");
    let myType = assembly.GetType("Qwantalabs.DummyAssembly.Say", true) 
    let method = myType.GetMethod("hello", Reflection.BindingFlags.Static)
    let result = method.Invoke(null,null)    
    myloader.Unload()
    0 // return an integer exit code
