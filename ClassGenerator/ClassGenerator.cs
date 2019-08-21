using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;


namespace ClassGenerator
{
    public class ClassGenerator
    {
        public string ClassName { get; set; }

        public string JsonData { get; set; }



        public List<dynamic> Generate()
        {
            /* Assambly Process Start */
            if (DynamicType.asmBuilder == null) DynamicType.GenerateAssemblyAndModule();
            var finalType = DynamicType.modBuilder.GetType(ClassName);
            TypeBuilder tb = DynamicType.CreateType(DynamicType.modBuilder, ClassName);
            /* Assambly Process Finish */



            JsonData = File.ReadAllText(@"D:\ContractFill-5115.json"); ;

            var dynamicObject = JsonConvert.DeserializeObject<JObject>(JsonData);
            var listDynamicObject = dynamicObject.Properties();



            DynamicType.CreateClassProperties(listDynamicObject, tb, out finalType);
            finalType = tb.CreateType();
            /* Propery create Finish */



            Object obj = Activator.CreateInstance(finalType);


            /* Propery set value Start */
            obj = DynamicType.PropertySetValue(listDynamicObject, finalType, obj);
            //foreach (var e in listDynamicObject)
            //{
            //    if (e.Value.Count() == 0)
            //    {
            //        string pname = e.Name;
            //        object value = e.Value.ToString();
            //        finalType.InvokeMember(pname, BindingFlags.SetProperty, null, obj, new object[] { value });
            //    }
            //}
            /* Propery set value Finish */

            return new List<dynamic>() { obj };
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
                    assemblyName.Name = "MyAssembly";
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


            public static void CreateClassProperties(IEnumerable<JProperty> properties, TypeBuilder typeBuilder, out Type type)
            {

                foreach (var e in properties)
                {
                    string pname = e.Name;

                    if (e.Value.Count() == 0)
                    {
                        string ptype = "System.String";
                        DynamicType.CreateProperty(typeBuilder, pname, Type.GetType(ptype));
                    }
                    else
                    {
                        TypeBuilder tbParent = DynamicType.CreateType(DynamicType.modBuilder, pname);

                        CreateClassProperties(e.Value.Children<JProperty>(), tbParent, out type);

                        DynamicType.CreateProperty(typeBuilder, pname, tbParent.GetType());
                    }

                }

                type = typeBuilder.CreateType();
            }

            public static object PropertySetValue(IEnumerable<JProperty> properties, Type finalType, object obj)
            {
                foreach (var e in properties)
                {
                    if (e.Value.Count() == 0)
                    {
                        string pname = e.Name;
                        object value = e.Value.ToString();
                        finalType.InvokeMember(pname, BindingFlags.SetProperty, null, obj, new object[] { value });
                    }
                    else
                    {
                        //var typer = finalType.GetProperty(e.Name).PropertyType;
                        //obj = PropertySetValue(e.Value.Children<JProperty>(), typer, obj);
                    }
                }

                return obj;
            }

        }
    }


}
