using System;
using System.Runtime.CompilerServices;

namespace FreeAwait
{
    public interface IRunnable
    {
        IRunnable? Run(IRunner runner);
    }

    public class Planner<TResult> : INotifyCompletion, IRunnable
    {

        public static Planner<TResult> Create() => new();

        public Planner()
        {
            Task = new Result<TResult>(this);
        }

        public Planner(IStep<TResult> step): this()
        {
            _step = step;
        }

        // awaiter

        public bool IsCompleted { get; private set; }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        public TResult GetResult() =>
            _error is null
                ? (IsCompleted
                    ? _result!
                    : throw new InvalidOperationException("Step is not completed"))
                : throw new SystemException("Running error", _error);

        // builder

        public IStep<TResult> Task { get; private set; }

        public void SetResult(TResult result)
        {
            _result = result;
            IsCompleted = true;
            _continuation?.Invoke();
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine =>
            stateMachine.MoveNext();

        public void SetStateMachine(IAsyncStateMachine _)
        {
        }

        public void SetException(Exception exception)
        {
            _error = exception;
            IsCompleted = true;
            _continuation?.Invoke();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
            if (awaiter is IRunnable runnable)
            {
                if (_runner is null)
                {
                    _pending = runnable;
                }
                else
                {
                    Use(_runner, runnable);
                }
            }
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine =>
            AwaitOnCompleted(ref awaiter, ref stateMachine);

        public Planner<TResult> Use(IRunner runner)
        {
            Use(runner, this);
            return this;
        }

        public IRunnable? Run(IRunner runner)
        {
            _runner = runner;
            if(_pending is not null)
            {
                return _pending.Run(runner);
            }

            _step = _step?.Run(runner, SetResult);
            return !IsCompleted && _step is not null ? this : null;
        }

        private static void Use(IRunner runner, IRunnable? runnable)
        {
            for (; runnable is not null; runnable = runnable.Run(runner)) ;
        }

        private TResult? _result;
        private Exception? _error;
        private IRunner? _runner;
        private IRunnable? _pending;
        private IStep<TResult>? _step;
        private Action? _continuation;
    }
}