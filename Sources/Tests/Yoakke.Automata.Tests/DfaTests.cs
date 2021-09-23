using System.Linq;
using Xunit;
using Yoakke.Automata.Sparse;

namespace Yoakke.Automata.Tests
{
    public class DfaTests : AutomatonTestBase
    {
        [Theory]
        [InlineData(new string[] { }, false)]
        [InlineData(new string[] { "a -> A" }, false)]
        [InlineData(new string[] { "b -> B" }, false)]
        [InlineData(new string[] { "a -> A", "a -> AA" }, false)]
        [InlineData(new string[] { "a -> A", "b -> AB" }, true)]
        [InlineData(new string[] { "b -> B", "b -> BB" }, false)]
        [InlineData(new string[] { "b -> B", "b -> BB", "a -> BA" }, true)]
        [InlineData(new string[] { "a -> A", "b -> AB", "a -> BA" }, true)]
        [InlineData(new string[] { "a -> A", "b -> AB", "a -> BA", "a -> AA" }, false)]
        [InlineData(new string[] { "a -> A", "b -> AB", "a -> BA", "b -> AB" }, true)]
        public void Last2DifferentAcceptsTests(string[] transitionTexts, bool accepts)
        {
            var dfa = BuildLast2DifferentCharsDfa();

            var transitions = transitionTexts.Select(ParseTransition).ToList();

            var state = dfa.InitialState;
            foreach (var (inputChar, expectedNextState) in transitions)
            {
                Assert.True(dfa.TryGetTransition(state!, inputChar, out var nextState));
                Assert.Equal(expectedNextState, nextState);
                state = nextState;
            }

            var input = transitions.Select(t => t.Item1);
            Assert.Equal(accepts, dfa.Accepts(input));
        }

        [Theory]
        [InlineData(new string[] { }, false)]
        [InlineData(new string[] { "a -> A, AA" }, false)]
        [InlineData(new string[] { "b -> B, BB" }, false)]
        [InlineData(new string[] { "a -> A, AA", "a -> A, AA" }, false)]
        [InlineData(new string[] { "a -> A, AA", "b -> AB" }, true)]
        [InlineData(new string[] { "b -> B, BB", "b -> B, BB" }, false)]
        [InlineData(new string[] { "b -> B, BB", "b -> B, BB", "a -> BA" }, true)]
        [InlineData(new string[] { "a -> A, AA", "b -> AB", "a -> BA" }, true)]
        [InlineData(new string[] { "a -> A, AA", "b -> AB", "a -> BA", "a -> A, AA" }, false)]
        [InlineData(new string[] { "a -> A, AA", "b -> AB", "a -> BA", "b -> AB" }, true)]
        public void Last2DifferentAcceptsMinimizedTests(string[] transitionTexts, bool accepts)
        {
            var dfa = BuildLast2DifferentCharsDfa().Minimize();

            var transitions = transitionTexts.Select(ParseTransition).ToList();

            var state = dfa.InitialState;
            foreach (var (inputChar, expectedNextStateText) in transitions)
            {
                var expectedNextState = ParseStateSet(expectedNextStateText);
                Assert.True(dfa.TryGetTransition(state!, inputChar, out var nextState));
                Assert.Equal(expectedNextState, nextState);
                state = nextState;
            }

            var input = transitions.Select(t => t.Item1);
            Assert.Equal(accepts, dfa.Accepts(input));
        }

        [Fact]
        public void Last2DifferentMinimize()
        {
            var dfa = BuildLast2DifferentCharsDfa().Minimize();

            var expectedStates = new[] { "S", "A, AA", "B, BB", "AB", "BA" }.Select(ParseStateSet);
            var gotStates = dfa.States.ToHashSet();

            var expectedAcceptingStates = new[] { "AB", "BA" }.Select(ParseStateSet);
            var gotAcceptingStates = dfa.AcceptingStates.ToHashSet();

            Assert.True(gotStates.SetEquals(expectedStates));
            Assert.True(gotAcceptingStates.SetEquals(expectedAcceptingStates));

            Assert.Equal(10, dfa.Transitions.Count);
            AssertTransition(dfa, "S", 'a', "A, AA");
            AssertTransition(dfa, "S", 'b', "B, BB");
            AssertTransition(dfa, "A, AA", 'a', "A, AA");
            AssertTransition(dfa, "A, AA", 'b', "AB");
            AssertTransition(dfa, "B, BB", 'a', "BA");
            AssertTransition(dfa, "B, BB", 'b', "B, BB");
            AssertTransition(dfa, "AB", 'a', "BA");
            AssertTransition(dfa, "AB", 'b', "B, BB");
            AssertTransition(dfa, "BA", 'a', "A, AA");
            AssertTransition(dfa, "BA", 'b', "AB");
        }

        [Fact]
        public void CompleteSimple()
        {
            // We build a DFA that accepts a*
            // And we complete it over { a, b }
            var dfa = new Dfa<char, char>();
            dfa.InitialState = 'q';
            dfa.AcceptingStates.Add('q');
            dfa.AddTransition('q', 'a', 'q');

            Assert.True(dfa.Complete("ab", 't'));
            var expectedStates = new[] { 'q', 't' }.ToHashSet();
            Assert.True(expectedStates.SetEquals(dfa.States));
            Assert.Equal(4, dfa.Transitions.Count);
            AssertTransition(dfa, 'q', 'a', 'q');
            AssertTransition(dfa, 'q', 'b', 't');
            AssertTransition(dfa, 't', 'a', 't');
            AssertTransition(dfa, 't', 'b', 't');
        }

        private static Dfa<string, char> BuildLast2DifferentCharsDfa()
        {
            var dfa = new Dfa<string, char>();
            dfa.InitialState = "S";
            dfa.AcceptingStates.Add("AB");
            dfa.AcceptingStates.Add("BA");
            dfa.AddTransition("S", 'a', "A");
            dfa.AddTransition("S", 'b', "B");
            dfa.AddTransition("A", 'a', "AA");
            dfa.AddTransition("A", 'b', "AB");
            dfa.AddTransition("AA", 'a', "AA");
            dfa.AddTransition("AA", 'b', "AB");
            dfa.AddTransition("B", 'a', "BA");
            dfa.AddTransition("B", 'b', "BB");
            dfa.AddTransition("BB", 'a', "BA");
            dfa.AddTransition("BB", 'b', "BB");
            dfa.AddTransition("AB", 'a', "BA");
            dfa.AddTransition("AB", 'b', "BB");
            dfa.AddTransition("BA", 'a', "AA");
            dfa.AddTransition("BA", 'b', "AB");
            return dfa;
        }
    }
}