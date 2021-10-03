using System;
using System.Runtime.CompilerServices;

namespace FreeAwait
{
    internal interface IRunnable<out TThis>
    {
        TThis Use(IRunner runner);
    }

    public class Planner<TResult> : INotifyCompletion, IRunnable<Planner<TResult>>
    {

        public static Planner<TResult> Create() => new();

        public Planner()
        {
            Task = new Plan<TResult>(this);
        }

        public Planner(IStep<TResult> step)
        {
            Task = step;
        }

        // awaiter

        public bool IsCompleted { get; private set; }

        public void OnCompleted(Action continuation) => _continuation = continuation;

        public TResult GetResult() =>
            _error is null
                ? (IsCompleted
                    ? _result!
                    : throw new InvalidOperationException("Step is not completed"))
                : throw new SystemException("Running error", _error);

        // builder

        public IStep<TResult> Task { get; }

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
            if (awaiter is IRunnable<object> runnable)
            {
                if (_runner is null)
                {
                    _pending = runnable;
                }
                else
                {
                    runnable.Use(_runner);
                }
            }
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
            if (awaiter is IRunnable<object> runnable)
            {
                if (_runner is null)
                {
                    _pending = runnable;
                }
                else
                {
                    runnable.Use(_runner);
                }
            }
        }

        public Planner<TResult> Use(IRunner runner)
        {
            _runner = runner;

            if (Task is Plan<TResult>)
            {
                _pending?.Use(runner);
                _pending = null;
            }
            else if (!IsCompleted)
            {
                Task.Run(_runner, SetResult);
            }

            return this;
        }

        private TResult? _result;
        private Exception? _error;
        private IRunner? _runner;
        private IRunnable<object>? _pending;
        private Action? _continuation;
    }
}