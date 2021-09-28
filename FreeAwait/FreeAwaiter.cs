using System;
using System.Runtime.CompilerServices;

namespace FreeAwait
{
	internal interface IInterpretable<out TThis>
	{
		TThis Use(IInterpreter interpreter);
	}

	public class FreeAwaiter<TResult> : INotifyCompletion, IInterpretable<FreeAwaiter<TResult>>
	{

		public static FreeAwaiter<TResult> Create() => new();

		public FreeAwaiter()
		{
			Task = new Program<TResult>(this);
		}

		public FreeAwaiter(IInstruction<TResult> command)
		{
			Task = command;
		}

		// awaiter

		public bool IsCompleted { get; private set; }

		public void OnCompleted(Action continuation) => _continuation = continuation;

		public TResult GetResult() =>
			_error is null
				? (IsCompleted
					? _result!
					: throw new InvalidOperationException("Instruction is not completed"))
				: throw new SystemException("Interpretation error", _error);

		// builder

		public IInstruction<TResult> Task { get; }

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
			if (awaiter is IInterpretable<object> interpretable)
			{
				if (_interpreter is null)
				{
					_pending = interpretable;
				}
				else
				{
					interpretable.Use(_interpreter);
				}
			}
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
			ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : ICriticalNotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			awaiter.OnCompleted(stateMachine.MoveNext);
			if (awaiter is IInterpretable<object> interpretable)
			{
				if (_interpreter is null)
				{
					_pending = interpretable;
				}
				else
				{
					interpretable.Use(_interpreter);
				}
			}
		}

		public FreeAwaiter<TResult> Use(IInterpreter interpreter)
		{
			_interpreter = interpreter;

			if (Task is Program<TResult>)
			{
				_pending?.Use(interpreter);
				_pending = null;
			}
			else if (!IsCompleted)
			{
				Task.Run(_interpreter).ContinueWith(task => SetResult(task.Result));
			}

			return this;
		}

		private TResult? _result;
		private Exception? _error;
		private IInterpreter? _interpreter;
		private IInterpretable<object>? _pending;
		private Action? _continuation;
	}
}