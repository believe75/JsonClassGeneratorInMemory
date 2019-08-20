using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassGenerator
{
    public class ClassGenerator
    {
        public string ClassName { get; set; }

        public Dictionary<string,string> Properties { get; set; }



        public string Generate()
        {
            string a = "";

            var dataa = a.GetType().FullName;


            Properties = new Dictionary<string, string>();
            Properties.Add("test", "işlem");
            Properties.Add("test2", "işlem2");
            Properties.Add("test3", "işlem3");



            if (DynamicType.asmBuilder == null)
                DynamicType.GenerateAssemblyAndModule();


            var finalType = DynamicType.modBuilder.GetType("Beacon11");

            TypeBuilder tb = DynamicType.CreateType(DynamicType.modBuilder, "ClassName");

            foreach (var e in Properties)
            {
                string pname = e.Key;
                string ptype = "System.String";
                DynamicType.CreateProperty(tb, pname, Type.GetType(ptype));
            }
            finalType = tb.CreateType();

            Object obj = Activator.CreateInstance(finalType);
            // this sets the properties of the just instantiated class
            //finalType.InvokeMember("bv", BindingFlags.SetProperty, null, data, new object[] { 1.0 });
            //finalType.InvokeMember("tp", BindingFlags.SetProperty, null, data, new object[] { 2.0 });
            //this sets the properties of the type by using values from the querystring




            foreach (var e in Properties)
            {
                string pname = e.Key;
                object value = e.Value;
                finalType.InvokeMember(pname, BindingFlags.SetProperty, null, obj, new object[] { value });
            }































            // Create a Type Builder that generates a type directly into the current AppDomain.
            var appDomain = AppDomain.CurrentDomain;
            var assemblyName = new AssemblyName("MyDynamicAssembly");
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var typeBuilder = moduleBuilder.DefineType("MyDynamicType", TypeAttributes.Class | TypeAttributes.Public);

            var propertyName = "Name";

            // Generate a property called &quot;Name&quot;

            var propertyType = typeof(string);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Generate getter method

            var getter = typeBuilder.DefineMethod("Name", MethodAttributes.Public, propertyType, Type.EmptyTypes);

            var il = getter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);        // Push &quot;this&quot; on the stack
            il.Emit(OpCodes.Ret);            // Return

            propertyBuilder.SetGetMethod(getter);

            // Generate setter method

            var setter = typeBuilder.DefineMethod("Name", MethodAttributes.Public, null, new[] { propertyType });

            il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);        // Push &quot;this&quot; on the stack
            il.Emit(OpCodes.Ldarg_1);        // Push &quot;value&quot; on the stack
            il.Emit(OpCodes.Ret);            // Return

            propertyBuilder.SetSetMethod(setter);

            var t2 = assemblyBuilder.CodeBase;
            assemblyBuilder.Save("MyDynamicAssembly.dll");



            return "";
        }


        public class DynamicType
        {

            public static AssemblyBuilder asmBuilder = null;
            public static ModuleBuilder modBuilder = null;
            public static void GenerateAssemblyAndModule()
            {
                if (asmBuilder == null)
                {
                    AssemblyName assemblyName = new AssemblyName();
                    assemblyName.Name = "DWBeacons";
                    AppDomain thisDomain = Thread.GetDomain();
                    asmBuilder = thisDomain.DefineDynamicAssembly(
                                 assemblyName, AssemblyBuilderAccess.Run);
                    modBuilder = asmBuilder.DefineDynamicModule(
                                 asmBuilder.GetName().Name, false);
                }
            }

            public static TypeBuilder CreateType(ModuleBuilder modBuilder, string typeName)
            {
                TypeBuilder typeBuilder = modBuilder.DefineType(typeName,
                            TypeAttributes.Public |
                            TypeAttributes.Class |
                            TypeAttributes.AutoClass |
                            TypeAttributes.AnsiClass |
                            TypeAttributes.BeforeFieldInit |
                            TypeAttributes.AutoLayout,
                            typeof(object));
                return typeBuilder;
            }


            public static void CreateProperty(TypeBuilder t, string name, Type typ)
            {
                string field = "_" + name.ToLower();
                FieldBuilder fieldBldr = t.DefineField(field, typ, FieldAttributes.Private);
                PropertyBuilder propBldr = t.DefineProperty(name, PropertyAttributes.HasDefault, typ, null);
                MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                MethodBuilder getPropBldr = t.DefineMethod("get_" + name, getSetAttr, typ, Type.EmptyTypes);

                ILGenerator getIL = getPropBldr.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBldr);
                getIL.Emit(OpCodes.Ret);

                MethodBuilder setPropBldr = t.DefineMethod("set_" + name, getSetAttr, null, new Type[] { typ });

                ILGenerator setIL = setPropBldr.GetILGenerator();

                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBldr);
                setIL.Emit(OpCodes.Ret);

                propBldr.SetGetMethod(getPropBldr);
                propBldr.SetSetMethod(setPropBldr);

            }

        }
    }
}
