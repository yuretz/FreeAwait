using FreeAwait;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    public class Repository : 
        IRun<Create, Todo>,
        IRun<Read, Todo?>,
        IRun<ReadAll, IEnumerable<Todo>>

    {   
        public Repository(Store store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IEnumerable<Todo> Run(ReadAll step) => _store.Todos.Select(pair => pair.Value).OrderBy(item => item.Order);

        public Todo Run(Create step)
        {
            var item = new Todo(_store.NextId(), step.Title, false, step.Order);
            _store.Todos[item.Id] = item;
            
            return item;
        }

        public Todo? Run(Read step) => _store.Todos.TryGetValue(step.Id, out var item) ? item : default;
        
        private readonly Store _store;
    }
}
