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

        private record Count(string Text): IStep<Count, int>;
        private record Twice(int Number): IStep<Twice, int>;
        private record ThreeMore(int Number): IStep<ThreeMore, int>;

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
    }
}
