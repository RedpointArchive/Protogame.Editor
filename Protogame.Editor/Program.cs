#if FALSE

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyConfiguration("patched")]

namespace Protogame.Editor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Check that we don't have MonoGame in our appdomain.  If we do, then it's too late
            // and we can't patch.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Any(x => x.GetName().Name == "MonoGame.Framework"))
            {
                throw new InvalidOperationException("MonoGame is already loaded, unable to patch!");
            }

            // Patch MonoGame to use marshalled objects if needed.
            /*PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.D3DCompiler.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.DXGI.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.Direct2D1.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.Direct3D11.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.MediaFoundation.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.RawInput.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.XAudio2.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "SharpDX.XInput.dll"));
            PatchAssembly(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "MonoGame.Framework.dll"));*/
            
            // Launch the editor.
            global::Program.GameMain(args);
        }

        private static void PatchProgress(string message)
        {
            //Debug.WriteLine(message);
            //Console.WriteLine(message);
        }

        private static void PatchAssembly(string mgLocation)
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(mgLocation, new ReaderParameters { ReadWrite = true }))
            {
                foreach (var module in assembly.Modules)
                {
                    foreach (var type in module.Types)
                    {
                        if (type.BaseType != null &&
                            type.BaseType.FullName == "System.Object")
                        {
                            PatchProgress("Patched " + type.FullName + " to be marshallable");
                            type.BaseType = module.ImportReference(typeof(MarshalByRefObject));
                        }
                        else if (type.BaseType != null &&
                            type.BaseType.FullName == "System.ValueType")
                        {
                            if (!type.CustomAttributes.Any(x => x.AttributeType.FullName == "System.SerializableAttribute"))
                            {
                                PatchProgress("Patched " + type.FullName + " to be serializable");
                                var serializableAttribute = module.ImportReference(typeof(SerializableAttribute));
                                var constructor = module.ImportReference(serializableAttribute.Resolve().Methods.First(x => x.IsConstructor));
                                type.CustomAttributes.Add(new CustomAttribute(constructor));
                                type.IsSerializable = true;
                            }

                            foreach (var nestedType in type.NestedTypes)
                            {
                                if (nestedType.BaseType != null &&
                                    nestedType.BaseType.FullName == "System.ValueType")
                                {
                                    if (!nestedType.CustomAttributes.Any(x => x.AttributeType.FullName == "System.SerializableAttribute"))
                                    {
                                        PatchProgress("Patched " + nestedType.FullName + " to be serializable");
                                        var serializableAttribute = module.ImportReference(typeof(SerializableAttribute));
                                        var constructor = module.ImportReference(serializableAttribute.Resolve().Methods.First(x => x.IsConstructor));
                                        nestedType.CustomAttributes.Add(new CustomAttribute(constructor));
                                        nestedType.IsSerializable = true;
                                    }
                                }
                            }
                        }

                        foreach (var field in type.Fields)
                        {
                            PatchProgress("Patched " + type.FullName + "." + field.Name + " to be public for remoting");
                            field.IsPublic = true;

                            try
                            {
                                var fieldType = field.FieldType.Resolve();
                                if (fieldType != null && fieldType.Module == module)
                                {
                                    PatchProgress("Patched " + fieldType.FullName + " to be public for remoting");
                                    if (fieldType.IsNested)
                                    {
                                        fieldType.IsNestedPublic = true;
                                    }
                                    else
                                    {
                                        fieldType.IsPublic = true;
                                    }
                                }
                            }
                            catch (AssemblyResolutionException)
                            {
                            }
                        }
                            /*foreach (var prop in type.Properties)
                            {
                                var madePublic = false;
                                if (prop.GetMethod != null/* && prop.GetMethod.IsAssembly/)
                                {
                                    PatchProgress("Patched " + type.FullName + "." + prop.Name + "(get) to be public for remoting");
                                    prop.GetMethod.IsPublic = true;
                                    //prop.GetMethod.IsAssembly = false;
                                    madePublic = true;
                                }
                                if (prop.SetMethod != null/* && prop.SetMethod.IsAssembly*)
                                {
                                    PatchProgress("Patched " + type.FullName + "." + prop.Name + "(set) to be public for remoting");
                                    prop.SetMethod.IsPublic = true;
                                    //prop.SetMethod.IsAssembly = false;
                                    madePublic = true;
                                }

                                if (madePublic)
                                {
                                    var propType = prop.PropertyType.Resolve();
                                    if (propType != null)
                                    {
                                        if (/*!propType.IsPublic && *propType.Module == module)
                                        {
                                            PatchProgress("Patched " + propType.FullName + " to be public for remoting");
                                            if (propType.IsNested)
                                            {
                                                propType.IsNestedPublic = true;
                                            }
                                            else
                                            {
                                                propType.IsPublic = true;
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (var method in type.Methods)
                            {
                                if (method.IsAssembly)
                                {
                                    PatchProgress("Patched " + type.FullName + "." + method.Name + " to be public for remoting");
                                    method.IsPublic = true;
                                    //method.IsAssembly = false;

                                    var returnType = method.ReturnType.Resolve();
                                    if (returnType.Module == module)
                                    {
                                        PatchProgress("Patched " + returnType.FullName + " to be public for remoting");
                                        if (returnType.IsNested)
                                        {
                                            returnType.IsNestedPublic = true;
                                        }
                                        else
                                        {
                                            returnType.IsPublic = true;
                                        }
                                    }

                                    foreach (var p in method.Parameters)
                                    {
                                        var paraType = p.ParameterType.Resolve();
                                        if (paraType.Module == module)
                                        {
                                            PatchProgress("Patched " + paraType.FullName + " to be public for remoting");
                                            if (paraType.IsNested)
                                            {
                                                paraType.IsNestedPublic = true;
                                            }
                                            else
                                            {
                                                paraType.IsPublic = true;
                                            }
                                        }
                                    }
                                }
                            }*/
                    }
                }

                assembly.Write();
            }
        }
    }
}

#endif