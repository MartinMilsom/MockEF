using Castle.Core.Interceptor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MockEF.Interceptors
{
    public class DbSetInterceptor : IInterceptor
    {
        private readonly IQueryable<object> _data;
        private readonly InMemoryDataProvider _dataProvider;
        private readonly Dictionary<string, Action<IInvocation>> _actions;

        public DbSetInterceptor(IQueryable<object> data, InMemoryDataProvider dataProvider)
        {
            _data = data;
            _dataProvider = dataProvider;
            //TODO: can we compare method itself, and not rely on name string?
            _actions = new Dictionary<string, Action<IInvocation>>
            {
                { "Add", AddToInMemoryStore },
                { "Remove", RemoveFromInMemoryStore },
                { "Find", FindFromInMemoryStore },
                { "Attach", AddToInMemoryStore },
                { "Create", CreateNew },
                { "get_Expression", x => x.ReturnValue = _data.Expression },
                { "get_Provider", x => x.ReturnValue = _data.Provider },
                { "get_ElementType", x => x.ReturnValue = _data.ElementType },
                { "GetEnumerator", x => x.ReturnValue = _data.GetEnumerator() },

            };
        }

        public void Intercept(IInvocation invocation)
        {
            if (_actions.ContainsKey(invocation.Method.Name))
            {
                _actions[invocation.Method.Name](invocation);
            }
        }

        private void AddToInMemoryStore(IInvocation invocation)
        {
            var type = invocation.Method.ReturnType;
            _dataProvider.Add(type, invocation.Arguments[0]);
        }

        private void RemoveFromInMemoryStore(IInvocation invocation)
        {
            var type = invocation.Method.ReturnType;
            var arg = invocation.Arguments[0];
            _dataProvider.Remove(type, arg);
        }

        private void FindFromInMemoryStore(IInvocation invocation)
        {
            var type = invocation.Method.ReturnType;
            var args = ((object[])invocation.Arguments[0]).ToList();
            var source = _dataProvider.Get(type);

            foreach (var record in source)
            {
                var keys = type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
                var values = keys.Select(k => k.GetValue(record)).ToList();

                args.OrderBy(o => o);
                values.OrderBy(o => o);

                if (!args.All(arg => values.ElementAt(args.IndexOf(arg)).Equals(arg))) continue;

                invocation.ReturnValue = record;
                break;
            }
        }

        private void CreateNew(IInvocation invocation)
        {
            //TODO: set identity where appropriate
            invocation.ReturnValue = Activator.CreateInstance(invocation.Method.ReturnType);
        }
    }
}
