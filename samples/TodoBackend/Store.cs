using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TodoBackend
{
    public class Store
    {
        public readonly ConcurrentDictionary<int, Todo> Todos = new();

        public int NextId() => Interlocked.Increment(ref _id);

        private int _id; 
    }
}
