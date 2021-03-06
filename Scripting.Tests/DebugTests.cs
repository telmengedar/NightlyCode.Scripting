﻿using System;
using System.Diagnostics;
using NightlyCode.Scripting;
using NightlyCode.Scripting.Data;
using NightlyCode.Scripting.Errors;
using NightlyCode.Scripting.Extensions;
using NightlyCode.Scripting.Parser;
using NightlyCode.Scripting.Providers;
using NightlyCode.Scripting.Tokens;
using NUnit.Framework;

namespace Scripting.Tests {

    /// <summary>
    /// tests debug functionality of script
    /// </summary>
    [TestFixture, Parallelizable]
    public class DebugTests {
        readonly IScriptParser parser = new ScriptParser(new Variable("lockedvar"));

        [Test, Parallelizable]
        public void SimpleAssignmentFailure() {
            IScript script = parser.Parse("$lockedvar=4");
            try {
                script.Execute();
                Assert.Fail("Script should fail with runtime exception");
            }
            catch (ScriptRuntimeException e) {
                Console.WriteLine(e.CreateStackTrace());
                Assert.That(e.Token is ICodePositionToken);
                Assert.AreEqual(1, ((ICodePositionToken) e.Token).LineNumber);
            }
        }

        [Test, Parallelizable]
        public void ExternalMethodFail() {
            ((ScriptParser) parser).MethodResolver = new ResourceScriptMethodProvider(typeof(DebugTests).Assembly, parser);

            IScript script = parser.Parse(ScriptCode.Create(
                "// import method which fails in the end",
                "$method=import(\"Scripting.Tests.Scripts.External.fail.ns\")",
                "// now call failing method",
                "$method()"
            ));

            try {
                script.Execute();
                Assert.Fail("Script should fail with runtime exception");
            }
            catch (ScriptRuntimeException e) {
                Console.WriteLine(e.CreateStackTrace());
                Assert.That(e.Token is ICodePositionToken);
                Assert.AreEqual(4, ((ICodePositionToken) e.Token).LineNumber);
            }
        }
    }
}