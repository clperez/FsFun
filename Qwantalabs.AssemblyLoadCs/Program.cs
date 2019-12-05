using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace example
{
    class TestAssemblyLoadContext : AssemblyLoadContext
    {
        public TestAssemblyLoadContext() : base(true)
        {
        }

        //protected override Assembly Load(AssemblyName name)
        //{
        //    return null;
        //}
    }

    class Manager
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(out WeakReference testAlcWeakRef)
        {
            var alc = new TestAssemblyLoadContext();
            testAlcWeakRef = new WeakReference(alc);
            alc.Resolving += (alc2, assemblyName) =>
            {
                var dllName = assemblyName.Name.Split(',').First();

                var dllPath = dllName.Contains("FSharp.Core")
                    ? @"C:\projects\FsFun\Qwantalabs.LoadAssemblies\bin\Release\netcoreapp3.1\FSharp.Core.dll"
                    : @$"C:\projects\FsFun\Qwantalabs.DummyAssembly\bin\Debug\netstandard2.0\{dllName}.dll";
                                
                return alc2.LoadFromAssemblyPath(dllPath);
            };

//            Assembly a = alc.LoadFromAssemblyPath("C:\\projects\\FsFun\\Qwantalabs.DummyAssembly\\bin\\Debug\\netstandard2.0\\Qwantalabs.DummyAssembly.dll");
            var assembly = alc.LoadFromAssemblyPath("C:\\projects\\FsFun\\Qwantalabs.DummyAssembly\\bin\\Debug\\netstandard2.0\\Qwantalabs.DummyAssembly.dll");
            var myType = assembly.GetType("Qwantalabs.DummyAssembly.Say", true);
            var method = myType.GetMethod("hello", BindingFlags.Public | BindingFlags.Static);
            var result = method.Invoke(null, new Object[] {"Carlos"});

            alc.Unload();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Unload(WeakReference testAlcWeakRef)
        {
            for (int i = 0; testAlcWeakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Console.WriteLine($"is alive: {testAlcWeakRef.IsAlive}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var manager = new Manager();
            manager.Execute(out var testAlcWeakRef);
            manager.Unload(testAlcWeakRef);
        }
    }
}