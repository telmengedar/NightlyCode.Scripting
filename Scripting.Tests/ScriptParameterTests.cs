﻿using System.Linq;
using NightlyCode.Scripting;
using NightlyCode.Scripting.Data;
using NightlyCode.Scripting.Extensions;
using NightlyCode.Scripting.Parser;
using NightlyCode.Scripting.Visitors;
using NUnit.Framework;

namespace Scripting.Tests {

    [TestFixture, Parallelizable]
    public class ScriptParameterTests {
        readonly IScriptParser parser = new ScriptParser();

        [Test, Parallelizable]
        public void DetectOptionalScriptParameter() {
            IScript script = parser.Parse("parameter($parameter, \"int\", 0) method.call($parameter)", new Variable("method"));
            ParameterExtractor extractor=new ParameterExtractor();
            extractor.Visit(script);
            Assert.AreEqual(1, extractor.Parameters.Count());
            Assert.AreEqual("parameter", extractor.Parameters.First().Name);
            Assert.That(extractor.Parameters.First().IsOptional);
        }

        [Test, Parallelizable]
        public void DetectScriptParameter() {
            IScript script = parser.Parse("method.call($parameter)", new Variable("method"));
            ParameterExtractor extractor=new ParameterExtractor();
            extractor.Visit(script);
            Assert.AreEqual(1, extractor.Parameters.Count());
            Assert.AreEqual("parameter", extractor.Parameters.First().Name);
            Assert.AreEqual(false, extractor.Parameters.First().IsOptional);
        }

        [Test, Parallelizable]
        public void DetectVariableInitialization() {
            IScript script = parser.Parse(ScriptCode.Create(
                "$parameter=8",
                "method.call($parameter)"
            ), new Variable("method"));
            ParameterExtractor extractor=new ParameterExtractor();
            extractor.Visit(script);
            Assert.That(!extractor.Parameters.Any());
        }

        [Test, Parallelizable]
        public void DetectParameterAfterVariableInitializationInInnerBlock() {
            IScript script = parser.Parse(ScriptCode.Create(
                "if(20) {",
                "  $parameter=8",
                "}",
                "method.call($parameter)"
            ), new Variable("method"));
            ParameterExtractor extractor=new ParameterExtractor();
            extractor.Visit(script);
            Assert.AreEqual(1, extractor.Parameters.Count());
            Assert.AreEqual("parameter", extractor.Parameters.First().Name);
            Assert.AreEqual(false, extractor.Parameters.First().IsOptional);
        }

        [Test, Parallelizable]
        public void DetectImplicitParameterInSwitch() {
            IScript script = parser.Parse(ScriptCode.Create(
                "switch($condition)",
                "case(1) { return (4) }"
            ));
            ParameterExtractor extractor=new ParameterExtractor();
            extractor.Visit(script);
            Assert.AreEqual(1, extractor.Parameters.Count());
            Assert.AreEqual("condition", extractor.Parameters.First().Name);
            Assert.AreEqual(false, extractor.Parameters.First().IsOptional);
        }

        [Test, Parallelizable]
        public void ExceptionInCatchBlockIsResolved() {
            IScript script = parser.Parse(ScriptCode.Create(
                "try {",
                "   method.call(3)",
                "}",
                "catch {",
                "   return($exception.message)",
                "}"
            ), new Variable("method"));

            ParameterExtractor extractor = new ParameterExtractor();
            extractor.Visit(script);
            Assert.AreEqual(0, extractor.Parameters.Count());
        }

        [Test, Parallelizable]
        public void ExceptionInCatchStatementIsResolved() {
            IScript script = parser.Parse(ScriptCode.Create(
                "try",
                "  method.call(3)",
                "catch",
                "  return($exception.message)"
            ), new Variable("method"));

            ParameterExtractor extractor = new ParameterExtractor();
            extractor.Visit(script);
            Assert.That(!extractor.Parameters.Any());
        }

        [Test, Parallelizable]
        public void NewlineBetweenCommentAndToken() {
            IScript script = parser.Parse(ScriptCode.Create(
                "// bla bla",
                "",
                "using(true)",
                "{",
                "\nif(true)",
                "\n\nreturn(0)",
                "}"
            ));

            ParameterExtractor extractor = new ParameterExtractor();
            extractor.Visit(script);
            Assert.That(!extractor.Parameters.Any());
        }

    }
}