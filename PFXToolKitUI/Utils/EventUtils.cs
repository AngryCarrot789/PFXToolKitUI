// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Linq.Expressions;
using System.Reflection;

namespace PFXToolKitUI.Utils;

public static class EventUtils {
    internal static MethodInfo? InvokeActionMethod;
    internal static readonly Dictionary<Type, ParameterExpression[]> DelegateTypeToParameters = new Dictionary<Type, ParameterExpression[]>();

    public static Delegate CreateDelegateToInvokeActionFromEvent(Type eventHandlerType, Action actionToInvoke) {
        // Get or create cached array of the eventType's parameters. Generic parameters cannot be handled currently
        ParameterExpression[] paramArray = GetCachedParameterExpressions(eventHandlerType);

        // This can't really be optimised any further
        // Creates a lambda, with the eventType's delegate method signature, that invokes actionToInvoke
        MethodCallExpression invokeAction = Expression.Call(Expression.Constant(actionToInvoke), InvokeActionMethod ??= (typeof(Action).GetMethod("Invoke") ?? throw new Exception("Missing Invoke method on action")));
        return Expression.Lambda(eventHandlerType, invokeAction, paramArray).Compile();
    }

    public static Delegate CreateDelegateToInvokeActionFromEvent(Type eventHandlerType, Delegate actionToInvokeWithParameter, Type typeOfParameter, object? extraParameterCall = null) {
        MethodInfo invoke = actionToInvokeWithParameter.GetType().GetMethod("Invoke") ?? throw new Exception("Missing Invoke method on action");
        ParameterInfo[] invokeParams = invoke.GetParameters();
        if (extraParameterCall != null) {
            if (invokeParams.Length != 2)
                throw new Exception("Handler delegate does not have exactly 2 parameters to accept the sender and custom parameter");
            if (invokeParams[1].ParameterType != typeof(object))
                throw new Exception("Handler delegate's custom parameter is not of type object");
        }
        else if (invokeParams.Length != 1)
            throw new Exception("Handler delegate does not have exactly 1 parameter to accept the sender");
        
        ParameterExpression[] paramArray = GetCachedParameterExpressions(eventHandlerType, typeOfParameter, static (typeT, md, parameters) => {
            if (parameters.Length < 1)
                throw new Exception("Event does not have any parameters. Cannot get the sender parameter");

            Type senderParamType = parameters[0].ParameterType;
            if (!senderParamType.IsAssignableFrom((Type) typeT!))
                throw new Exception($"Event's first parameter ({parameters[0].Name}, type {senderParamType.Name}) cannot be assigned to {((Type) typeT!).Name}");
        });

        Expression senderParam = paramArray[0].Type != typeOfParameter ? Expression.Convert(paramArray[0], typeOfParameter) : paramArray[0];
        ConstantExpression constActionToInvoke = Expression.Constant(actionToInvokeWithParameter);
        MethodCallExpression invokeHandler = extraParameterCall == null 
            ? Expression.Call(constActionToInvoke, invoke, senderParam) 
            : Expression.Call(constActionToInvoke, invoke, senderParam, Expression.Constant(extraParameterCall));
        
        return Expression.Lambda(eventHandlerType, invokeHandler, paramArray).Compile();
    }

    public static void CreateEventInterface<TTarget, TEvent>(EventInfo info, out Action<TTarget, TEvent> addHandler, out Action<TTarget, TEvent> removeHandler) where TEvent : Delegate {
        ParameterExpression paramTarget = Expression.Parameter(typeof(TTarget), "instance");
        ParameterExpression paramHandler = Expression.Parameter(typeof(TEvent), "handler");
        addHandler = CreateAddOrRemove<TTarget, TEvent>(paramTarget, info.AddMethod!, paramHandler);
        removeHandler = CreateAddOrRemove<TTarget, TEvent>(paramTarget, info.RemoveMethod!, paramHandler);
    }

    public static Action<TTarget, TEvent> CreateAddOrRemove<TTarget, TEvent>(MethodInfo addOrRemoveMethod) where TEvent : Delegate {
        ParameterExpression paramTarget = Expression.Parameter(typeof(TTarget), "instance");
        ParameterExpression paramHandler = Expression.Parameter(typeof(TEvent), "handler");
        return CreateAddOrRemove<TTarget, TEvent>(paramTarget, addOrRemoveMethod, paramHandler);
    }

    public static Action<TTarget, TEvent> CreateAddOrRemove<TTarget, TEvent>(ParameterExpression target, MethodInfo targetMethod, ParameterExpression handler) where TEvent : Delegate {
        return Expression.Lambda<Action<TTarget, TEvent>>(Expression.Call(target, targetMethod, handler), target, handler).Compile();
    }

    public static ParameterExpression[] GetCachedParameterExpressions(Type delegateType, object? extraParam = null, Action<object?, MethodInfo, ParameterInfo[]>? preProcess = null) {
        if (!DelegateTypeToParameters.TryGetValue(delegateType, out ParameterExpression[]? paramArray)) {
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke") ?? throw new Exception(delegateType.Name + " is not a delegate type");
            ParameterInfo[] parameters = invokeMethod.GetParameters();
            preProcess?.Invoke(extraParam, invokeMethod, parameters);
            DelegateTypeToParameters[delegateType] = paramArray = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        }

        return paramArray;
    }
}