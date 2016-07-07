using Castle.Core.Interceptor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MockEF.Interceptors
{
    public class ContextInterceptor : IInterceptor
    {
        private readonly Dictionary<Type, Func<object>> _callbacks;

        public ContextInterceptor(Dictionary<Type, Func<object>> callbacks)
        {
            _callbacks = callbacks;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!invocation.Method.ReturnType.GetInterfaces().Contains(typeof (IEnumerable)) ||
                    !invocation.Method.ReturnType.GetInterfaces().Contains(typeof (IQueryable)))
            {
                //Should only mock dbsets from this object. Anything else is handled externally.
                return;
            }

            var type = invocation.Method.ReturnType;
            var genericType = type.GetGenericArguments().First();

            if (!_callbacks.ContainsKey(genericType)) return;

            var proxy = _callbacks[genericType]();
            invocation.ReturnValue = proxy;
        }
    }
}
