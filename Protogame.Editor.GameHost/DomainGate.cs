using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Protogame.Editor.GameHost
{
    public class DomainGate : MarshalByRefObject
    {
        private GameLoader _gameLoader;

        public DomainGate(GameLoader gameLoader)
        {
            _gameLoader = gameLoader;
        }

        public static void Send(DomainGate gate, object o)
        {
            var oldMode = GCSettings.LatencyMode;
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.Batch;
                var addAndGcCount = ObjectAddress.GetAddress(o);
                gate.ReceiveObject(addAndGcCount.Value, addAndGcCount.Key);
            }
            finally
            {
                GCSettings.LatencyMode = oldMode;
            }
        }

        private void ReceiveObject(int gcCount, IntPtr intPtr)
        {
            var currentGcCount = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
            if (currentGcCount != gcCount)
            {
                throw new Exception("GC occurred during DomainGate setup");
            }

            var data = PtrConverter<Dictionary<string, object>>.ConvertFromIntPtr(intPtr);

            _gameLoader.AssignCrossDomainDataStorage(data);
        }

        public class PtrConverter<T>
        {
            delegate U Void2ObjectConverter<U>(IntPtr pManagedObject);
            static Void2ObjectConverter<T> myConverter;
            
            static PtrConverter()
            {
                GenerateDynamicMethod();
            }

            static void GenerateDynamicMethod()
            {
                if (myConverter == null)
                {
                    var method = new DynamicMethod("ConvertPtrToObjReference", typeof(T),
                                   new Type[] { typeof(IntPtr) }, StaticModule.UnsafeModule);
                    var gen = method.GetILGenerator();
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ret);
                    myConverter = (Void2ObjectConverter<T>)method.CreateDelegate(typeof(Void2ObjectConverter<T>));
                }
            }
            
            /// <exception cref="ExecutionEngineException">If the object has been moved by the GC, this exception is thrown.</exception>
            public static T ConvertFromIntPtr(IntPtr ptr)
            {
                return myConverter(ptr);
            }
        }

        private class StaticModule
        {
            private const string ModuleAssemblyName = "Protogame.GameHost.DynamicCaster";
            private static Module _unsafeModule;

            public static Module UnsafeModule
            {
                get
                {
                    if (_unsafeModule == null)
                    {
                        var assemblyName = new AssemblyName(ModuleAssemblyName);
                        var aBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                                                                                     AssemblyBuilderAccess.Run);
                        var mBuilder = aBuilder.DefineDynamicModule(ModuleAssemblyName);
                        var secAttrib = typeof(SecurityPermissionAttribute);
                        var secCtor = secAttrib.GetConstructor(new Type[] { typeof(SecurityAction) });
                        var attribBuilder = new CustomAttributeBuilder(secCtor,
                            new object[] { SecurityAction.Assert },
                            new PropertyInfo[] { secAttrib.GetProperty("SkipVerification", BindingFlags.Instance | BindingFlags.Public) },
                            new object[] { true });

                        aBuilder.SetCustomAttribute(attribBuilder);
                        var tb = mBuilder.DefineType("MyDynamicType", TypeAttributes.Public);
                        _unsafeModule = tb.CreateType().Module;
                    }

                    return _unsafeModule;
                }
            }
        }
    }
}
