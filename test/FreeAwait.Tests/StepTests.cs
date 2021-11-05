using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FreeAwait.Tests
{
    public class StepTests
    {
        [Fact]
        public async Task AwaitFromResult()
        {
            Assert.Equal(42, await Step.Result(42));
        }

        [Fact]
        public async Task AwaitFromResultAsync()
        {
            Assert.Equal(42, await f());

            static async IStep<int> f() => await Step.Result(42);       
        }

        [Fact]
        public async Task AwaitSingleStep()
        {
            Assert.Equal(
                new[] { 3, 2, 3, 1 },
                new[]
                {
                    await new Runner1().Run(f("foo")),
                    await new Runner2().Run(f("foo")),
                    await f("bar").Use(new Runner1()),
                    await f("bar").Use(new Runner2()),
                });

             static async IStep<int> f(string text) => await new Count(text);
        }

        [Fact]
        public async Task LeftResultIdentity()
        {

            Assert.Equal(await Step.Result(42).PassTo(f),  await f(42));

            static IStep<int> f(int value) => Step.Result(value + 1);
        }

        [Fact]
        public async Task RightResultIdentity()
        {
            Assert.Equal(await Step.Result(42), await Step.Result(42).PassTo(f));

            static IStep<int> f(int value) => Step.Result(value);
        }

        [Fact]
        public async Task LeftStepIdentity()
        {
            var runner = new Runner1();

            Assert.Equal(3, await runner.Run(f("foo")));
            Assert.Equal(await runner.Run(Step.Result("foo").PassTo(f)), await runner.Run(f("foo")));

            static IStep<int> f(string text) => new Count(text);
        }

        [Fact]
        public async Task RightStepIdentity()
        {
            var runner = new Runner1();
            Assert.Equal(3, await runner.Run((IStep<Count, int>)new Count("bar")));
            Assert.Equal(
                await runner.Run((IStep<Count, int>)new Count("bar")), 
                await runner.Run(((IStep<Count, int>)new Count("bar")).PassTo(f)));

            static IStep<int> f(int value) => Step.Result(value);
        }

        [Fact]
        public async Task Associativity1()
        {
            var runner = new Runner1();
            var seven = Step.Result(7);

            Assert.Equal(17, await runner.Run(seven.PassTo(x => new Twice(x)).PassTo(x => new ThreeMore(x))));

            Assert.Equal(
                await runner.Run(seven.PassTo(x => new Twice(x)).PassTo(x => new ThreeMore(x))),
                await runner.Run(seven.PassTo(x => new Twice(x).PassTo(y => new ThreeMore(y))))); 
        }

        [Fact]
        public async Task Associativity2()
        {
            var runner = new Runner1();

            Assert.Equal(13, await runner.Run(f2()));
            Assert.Equal(await runner.Run(f1()), await runner.Run(f2()));

            async IStep<int> f1() => await new ThreeMore(await f11());
            async IStep<int> f11() => await new Twice(await Step.Result(5));
            async IStep<int> f2() => await f21(await Step.Result(5));
            async IStep<int> f21(int x) => await new ThreeMore(await new Twice(x));
        }

        [Fact]
        public async Task UnknownStep()
        {
            var runner = new Runner1();

            await Assert.ThrowsAnyAsync<Exception>(async() => await runner.Run(f()));

            static async IStep<int> f() => await new Twice(2) + await new Unknown(42);
        }

        [Fact]
        public async Task SyncScenario()
        {
            var runner = new Runner1();
            Assert.Equal(42, await runner.Run(FortyTwo()));
        }

        [Fact]
        public async Task AsyncScenario()
        {
            var runner = new AsyncRunner1();
            Assert.Equal(42, await runner.Run(FortyTwo()));
        }

        [Fact]
        public async Task DynamicScenario()
        {
            var runner = new DynamicRunner1();
            Assert.Equal(42, await runner.Run(FortyTwo()));
        }

        [Fact]
        public async Task RecursiveScenario()
        {
            var runner = new RecursiveRunner();
            Assert.Equal(120, await runner.Run(new Factor(5)));
        }

        [Fact]
        public async Task RecursiveScenario2()
        {
            var runner = new RecursiveRunner();
            Assert.Equal(120, await runner.Run(Factor(5)));

            static async IStep<int> Factor(int N) => N <= 1 ? N : await Factor(N - 1) * N;
        }

        [Fact]
        public async Task TailRecursion1()
        {
            var runner = new RecursiveRunner();
            Assert.Equal(2111485077978050, await runner.Run(new Fib(75)));
        }

        [Fact]
        public async Task TailRecursion2()
        {
            var runner = new RecursiveRunner();
            Assert.Equal(50005000, await runner.Run(new Sum(10000)));
        }

        [Fact]
        public async Task SuspendStep()
        {
            var runner = new Runner1();

            Assert.Equal(3, await runner.Run(Step.Suspend(() => new Count("foo"))));
        }

        private async IStep<int> FortyTwo() =>
            await new Twice(await new ThreeMore(await new Twice(await new Count("forty two"))));

        private record Count(string Text): IStep<Count, int>;
        private record Twice(int Number): IStep<Twice, int>;
        private record ThreeMore(int Number): IStep<ThreeMore, int>;
        private record Unknown(int Number): IStep<Unknown, int>;

        private class Runner1 : 
            IRun<Count, int>,
            IRun<Twice, int>,
            IRun<ThreeMore, int>   
        {
            public int Run(Count step) => step.Text.Length;

            public int Run(Twice step) => step.Number * 2;

            public int Run(ThreeMore step) => step.Number + 3;
        }

        private class Runner2 : IRun<Count, int>
        {
            public int Run(Count step) => step.Text.Count("aeuio".Contains);
        }

        private class AsyncRunner1:
            IRunAsync<Count, int>,
            IRunAsync<Twice, int>,
            IRunAsync<ThreeMore, int>
        {
            public Task<int> RunAsync(Count step) => Task.Run(() => step.Text.Length);

            public Task<int> RunAsync(Twice step) => Task.Run(() => step.Number * 2);

            public Task<int> RunAsync(ThreeMore step) => Task.Run(() => step.Number + 3);
        }

        private class DynamicRunner1: IRunMany
        {
            public IStep? Run(IStep step, Action<object> next)
            {
                next(step switch
                {
                    Count count => count.Text.Length,
                    Twice twice => twice.Number * 2,
                    ThreeMore threeMore => threeMore.Number + 3,
                    _ => throw new NotSupportedException()
                });
                return default;
            }
        }

        private record Factor(int N) : IStep<Factor, int>;
        private record Fib(int N, long Current = 1, long Previous = 0) : IStep<Fib, long>;
        private record Sum(int N, int Result = 0): IStep<Sum, int>;

        private class RecursiveRunner : 
            IRunStep<Factor, int>,
            IRunStep<Fib, long>,
            IRunStep<Sum, int>
        {
            public async IStep<int> RunStep(Factor step) => step.N <= 1 ? 1 : await new Factor(step.N - 1) * step.N;

            public IStep<long> RunStep(Fib step) => step.N <= 1
                ? Step.Result(step.Current)
                : new Fib(step.N - 1, step.Current + step.Previous, step.Current);

            public IStep<int> RunStep(Sum step) => step.N <= 0 
                ? Step.Result(step.Result) 
                : new Sum(step.N - 1, step.Result + step.N);

        }

    }
}
