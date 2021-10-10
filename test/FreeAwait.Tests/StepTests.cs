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
                
            Assert.Equal(await runner.Run(Step.Result("foo").PassTo(f)), await runner.Run(f("foo")));

            static IStep<int> f(string text) => new Count(text);
        }

        [Fact]
        public async Task RightStepIdentity()
        {
            var runner = new Runner1();

            Assert.Equal(
                await runner.Run((IStep<Count, int>)new Count("bar")), 
                await runner.Run(((IStep<Count, int>)new Count("bar")).PassTo(f)));

            static IStep<int> f(int value) => Step.Result(value);
        }

        private record Count(string Text): IStep<Count, int>;

        private class Runner1 : IRun<Count, int>
        {
            public int Run(Count step) => step.Text.Length; 
        }

        private class Runner2 : IRun<Count, int>
        {
            public int Run(Count step) => step.Text.Count("aeuio".Contains);
        }



    }
}
