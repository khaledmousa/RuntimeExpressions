using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RuntimeExpressions.ParseResult;

namespace RuntimeExpressions.Tests
{
    [TestFixture]
    public class BasicExpressions
    {
        private EvaluationEngine _engine;        

        [SetUp]
        public void Init()
        {
            _engine = new EvaluationEngine();
        }

        [Test]
        public void Can_Handle_Basic_Integers()
        {
            Assert.AreEqual(3, _engine.Evaluate<int>("2 + 1"));
            Assert.AreEqual(-1, _engine.Evaluate<int>("4 - 5"));
            Assert.AreEqual(30, _engine.Evaluate<int>("3 * 10"));
            Assert.AreEqual(3, _engine.Evaluate<int>("6/2"));
        }                

        [Test]
        public void Can_Handle_Basic_Decimals()
        {
            Assert.AreEqual(3.65, _engine.Evaluate<decimal>("2.2 + 1.45"));
            Assert.AreEqual(9.9, _engine.Evaluate<decimal>("10.2 - 0.3"));
            Assert.AreEqual(2.6, _engine.Evaluate<decimal>("1.3 * 2"));
            Assert.AreEqual(1.2, _engine.Evaluate<decimal>("4.8/4"));
        }

        [Test]
        public void Can_Handle_Basic_String()
        {
            Assert.AreEqual("Hello, world!", _engine.Evaluate<string>("\"Hello, \" + \"world!\""));
            Assert.AreEqual("KATE", _engine.Evaluate<string>("\"KARATE\" - \"AR\""));
        }

        [Test]
        public void Can_Handle_Basic_Boolean()
        {
            Assert.AreEqual(false, _engine.Evaluate<bool>("true && false"));
            Assert.AreEqual(true, _engine.Evaluate<bool>("true && true"));
            Assert.AreEqual(true, _engine.Evaluate<bool>("true || false"));
            Assert.AreEqual(true, _engine.Evaluate<bool>("true || true"));
            Assert.AreEqual(false, _engine.Evaluate<bool>("false || false"));
        }

        [Test]
        public void Can_Handle_Precedence()
        {
            Assert.AreEqual(9, _engine.Evaluate<int>("2 * 2 + 5"));
            Assert.AreEqual(-4, _engine.Evaluate<int>("10 / 5 - 6"));
            Assert.AreEqual(12, _engine.Evaluate<int>("2 + 2 * 5"));
            Assert.AreEqual(4, _engine.Evaluate<int>("6 - 10 / 5"));
        }

        [Test]
        public void Can_Handle_Parentheses()
        {
            Assert.AreEqual(20, _engine.Evaluate<int>("(2 + 2) * 5"));
            Assert.AreEqual(-1, _engine.Evaluate<int>("(6 - 10) / 4"));
            Assert.AreEqual(20, _engine.Evaluate<int>("5 * (2 + 2)"));
            Assert.AreEqual(-1, _engine.Evaluate<int>("0.25 * (6 - 10)"));
        }

        [Test]
        public void Can_Evaluate_Assignments()
        {
            var assignment = "x = (2 + 2) * 5";
            var parseResult = _engine.ParseString(assignment);
            Assert.IsTrue(parseResult.IsAssignmentResult);
            Assert.AreEqual("x", (parseResult as AssignmentResult).Item.Variable);
            Assert.AreEqual(20, _engine.EvaluateExpression<int>((parseResult as AssignmentResult).Item.Expression));
        }
    }
}
