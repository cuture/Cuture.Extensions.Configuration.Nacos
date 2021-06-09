using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Nacos.Grpc.Messages;

namespace Nacos.Grpc
{
    internal static class RequestProcessorUtil
    {
        internal static Dictionary<Type, Func<object, Task<NacosResponse>>> CreateHandleFunctions(params Assembly[] assemblies)
        {
            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (assemblies.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assemblies));
            }
            var handleFunctionDescriptors = assemblies.SelectMany(m => m.GetTypes())
                                                      .Where(m => typeof(IRequestHandler).IsAssignableFrom(m) && m.IsClass && !m.IsAbstract && m.GetConstructors().Any(c => c.GetParameters().Length == 0))
                                                      .Where(m => m.HasImplementedGeneric(typeof(IRequestHandler<>)))
                                                      .SelectMany(m => m.FindAllHandleFunctionDescriptor())
                                                      .ToArray();

            var handleFunctions = new Dictionary<Type, Func<object, Task<NacosResponse>>>();

            foreach (var descriptor in handleFunctionDescriptors)
            {
                if (handleFunctions.ContainsKey(descriptor.MessageType))
                {
                    throw new ArgumentException($"消息 {descriptor.MessageType} 具有多个Handler实现，请检查实现");
                }
                var instance = descriptor.HandlerInstance;
                var function = descriptor.HandleFunction;
                Func<object, Task<NacosResponse>> handleFunction = message =>
                {
                    return function(instance, message);
                };
                handleFunctions.Add(descriptor.MessageType, handleFunction);
            }

            return handleFunctions;
        }

        private static IEnumerable<HandleFunctionDescriptor> FindAllHandleFunctionDescriptor(this Type type)
        {
            var handerInstance = (Activator.CreateInstance(type) as IRequestHandler)!;

            var interfaceTypes = GetImplementedGenerics(type, typeof(IRequestHandler<>));
            foreach (var interfaceType in interfaceTypes)
            {
                var messageType = interfaceType.GetGenericArguments().First();

                var dynamicMethod = new DynamicMethod($"Dynamic_Call_{nameof(IRequestHandler)}_{Guid.NewGuid():n}", typeof(Task<NacosResponse>), new[] { typeof(object), typeof(object) });

                var ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Isinst, interfaceType);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Isinst, messageType);
                ilGenerator.Emit(OpCodes.Callvirt, interfaceType.GetMethods().First());
                ilGenerator.Emit(OpCodes.Ret);

                var handleFunction = dynamicMethod.CreateDelegate<Func<object, object, Task<NacosResponse>>>();

                yield return new(handleFunction, handerInstance, interfaceType, messageType);
            }
        }

        private static IEnumerable<Type> GetImplementedGenerics(this Type type, Type generic)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (generic == null)
            {
                throw new ArgumentNullException(nameof(generic));
            }

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                foreach (var item in type.GetInterfaces().Where(IsTheRawGenericType).ToArray())
                {
                    yield return item;
                }
                type = type.BaseType!;
            }

            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        /// <summary>
        /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
        /// </summary>
        /// <param name="type">需要测试的类型。</param>
        /// <param name="generic">泛型接口类型，传入 typeof(IXxx&lt;&gt;)</param>
        /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
        private static bool HasImplementedGeneric(this Type type, Type generic)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (generic == null)
            {
                throw new ArgumentNullException(nameof(generic));
            }

            // 测试接口。
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType)
            {
                return true;
            }

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType)
                {
                    return true;
                }
                type = type.BaseType!;
            }

            // 没有找到任何匹配的接口或类型。
            return false;

            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        private class HandleFunctionDescriptor
        {
            public Func<object, object, Task<NacosResponse>> HandleFunction { get; set; }

            public IRequestHandler HandlerInstance { get; set; }

            public Type InterfaceType { get; set; }

            public Type MessageType { get; set; }

            public HandleFunctionDescriptor(Func<object, object, Task<NacosResponse>> handleFunction, IRequestHandler handlerInstance, Type interfaceType, Type messageType)
            {
                HandleFunction = handleFunction;
                HandlerInstance = handlerInstance;
                InterfaceType = interfaceType;
                MessageType = messageType;
            }
        }
    }
}