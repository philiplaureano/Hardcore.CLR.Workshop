using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;

namespace ILRewriter.Extensions
{
    public static class TypeDefinitionExtensions
    {
        
        public static MethodDefinition DefineMethod(this TypeDefinition typeDef, string methodName,
            MethodAttributes attributes,
            MethodCallingConvention callingConvention, Type returnType,
            params Type[] parameterTypes)
        {
            var method = new MethodDefinition(methodName, attributes, null)
            {
                CallingConvention = callingConvention
            };

            typeDef.Methods.Add(method);

            // Match the parameter types
            method.AddParameters(parameterTypes);

            // Match the return type
            method.SetReturnType(returnType);

            return method;
        }


        public static MethodDefinition DefineMethod(this TypeDefinition typeDef, string methodName,
            MethodAttributes attributes, Type returnType, Type[] parameterTypes,
            Type[] genericParameterTypes)
        {
            var method = new MethodDefinition(methodName, attributes, null);

            typeDef.Methods.Add(method);

            //Match the generic parameter types
            foreach (var genericParameterType in genericParameterTypes)
            {
                method.AddGenericParameter(genericParameterType);
            }

            // Match the parameter types
            method.AddParameters(parameterTypes);

            // Match the return type
            method.SetReturnType(returnType);

            return method;
        }

        public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType)
        {
            var parentType = typeof(object);

            return AddDefaultConstructor(targetType, parentType);
        }

        public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType, Type parentType)
        {
            var module = targetType.Module;
            var voidType = module.Import(typeof(void));
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig
                                   | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;


            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var objectConstructor = parentType.GetConstructor(flags, null, new Type[0], null);

            // Revert to the System.Object constructor
            // if the parent type does not have a default constructor
            if (objectConstructor == null)
                objectConstructor = typeof(object).GetConstructor(new Type[0]);

            var baseConstructor = module.Import(objectConstructor);

            // Define the default constructor
            var ctor = new MethodDefinition(".ctor", methodAttributes, voidType)
            {
                CallingConvention = MethodCallingConvention.StdCall,
                ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed
            };

            var IL = ctor.Body.GetILProcessor();

            // Call the constructor for System.Object, and exit
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Call, baseConstructor);
            IL.Emit(OpCodes.Ret);

            targetType.Methods.Add(ctor);

            return ctor;
        }

        public static void AddProperty(this TypeDefinition typeDef, string propertyName, Type propertyType)
        {
            var module = typeDef.Module;
            var typeRef = module.Import(propertyType);
            typeDef.AddProperty(propertyName, typeRef);
        }

        public static void AddProperty(this TypeDefinition typeDef, string propertyName,
            TypeReference propertyType)
        {
            var fieldName = string.Format("__{0}_backingField", propertyName);
            var actualField = new FieldDefinition(fieldName, FieldAttributes.Private,
                propertyType);


            typeDef.Fields.Add(actualField);


            FieldReference backingField = actualField;
            if (typeDef.GenericParameters.Count > 0)
                backingField = GetBackingField(fieldName, typeDef, propertyType);

            var getterName = string.Format("get_{0}", propertyName);
            var setterName = string.Format("set_{0}", propertyName);


            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig |
                                                MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                                                MethodAttributes.Virtual;

            var module = typeDef.Module;
            var voidType = module.Import(typeof(void));

            // Implement the getter and the setter
            var getter = AddPropertyGetter(propertyType, getterName, attributes, backingField);
            var setter = AddPropertySetter(propertyType, attributes, backingField, setterName, voidType);

            typeDef.AddProperty(propertyName, propertyType, getter, setter);
        }
        
        public static void AddProperty(this TypeDefinition typeDef, string propertyName, TypeReference propertyType,
            MethodDefinition getter, MethodDefinition setter)
        {
            var newProperty = new PropertyDefinition(propertyName,
                PropertyAttributes.Unused, propertyType)
            {
                GetMethod = getter,
                SetMethod = setter
            };

            typeDef.Methods.Add(getter);
            typeDef.Methods.Add(setter);
            typeDef.Properties.Add(newProperty);
        }
        
        public static MethodDefinition GetMethod(this TypeDefinition typeDef, string methodName)
        {
            var result = from MethodDefinition m in typeDef.Methods
                where m.Name == methodName
                select m;

            return result.FirstOrDefault();
        }
        
        private static FieldReference GetBackingField(string fieldName, TypeDefinition typeDef,
            TypeReference propertyType)
        {
            // If the current type is a generic type, 
            // the current generic type must be resolved before
            // using the actual field
            var declaringType = new GenericInstanceType(typeDef);
            foreach (var parameter in typeDef.GenericParameters)
            {
                declaringType.GenericArguments.Add(parameter);
            }

            return new FieldReference(fieldName, declaringType, propertyType);            ;
        }

        private static MethodDefinition AddPropertyGetter(TypeReference propertyType,
            string getterName, MethodAttributes attributes,
            FieldReference backingField)
        {
            var getter = new MethodDefinition(getterName, attributes, propertyType)
            {
                IsPublic = true,
                ImplAttributes = MethodImplAttributes.Managed | MethodImplAttributes.IL
            };

            var IL = getter.GetILGenerator();
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldfld, backingField);
            IL.Emit(OpCodes.Ret);

            return getter;
        }

        private static MethodDefinition AddPropertySetter(TypeReference propertyType, MethodAttributes attributes,
            FieldReference backingField, string setterName,
            TypeReference voidType)
        {
            var setter = new MethodDefinition(setterName, attributes, voidType)
            {
                IsPublic = true,
                ImplAttributes = MethodImplAttributes.Managed | MethodImplAttributes.IL
            };

            setter.Parameters.Add(new ParameterDefinition(propertyType));

            var IL = setter.GetILGenerator();
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldarg_1);
            IL.Emit(OpCodes.Stfld, backingField);
            IL.Emit(OpCodes.Ret);

            return setter;
        }
    }
}