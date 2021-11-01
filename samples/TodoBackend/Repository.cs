using FreeAwait;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    public class Repository : 
        IRun<ReadAll, IEnumerable<Todo>>
    {
        
        public Repository(List<Todo> items)
        {
            _items = items;
            _items.Add(new(1, "Test", false));
        }

        public IEnumerable<Todo> Run(ReadAll step) => _items;
        

        private readonly List<Todo> _items;
    }
}
