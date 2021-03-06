﻿using System;
using NightlyCode.Scripting;
using NightlyCode.Scripting.Data;
using NightlyCode.Scripting.Extensions;
using NightlyCode.Scripting.Parser;
using NUnit.Framework;
using Scripting.Tests.Data;

namespace Scripting.Tests {

    [TestFixture, Parallelizable]
    public class MethodCallTests {
        readonly IScriptParser parser = new ScriptParser();

        public string MethodWithSingleParameter(string prefix, Parameter parameter) {
            return $"{prefix}:{parameter.Name}={parameter.Value}";
        }

        public string MethodWithArrayParameters(string prefix, params Parameter[] parameters)
        {
            return $"{prefix}:{string.Join<Parameter>(",", parameters)}";
        }

        public string MethodWithDefaults(string name, string title = null, params string[] additional) {
            return $"{title} {name} {string.Join(" ", additional)}";
        }

        public string Ambigious(byte[] bytearray) {
            return "bytearray";
        }

        public string Ambigious(string parameter) {
            return "string";
        }

        public string Ambigious(int parameter) {
            return "int";
        }

        public string InterfaceParameter(IParameter parameter)
        {
            return "parameter";
        }

        public string EnumParameter(TestEnum @enum) {
            return "enum";
        }

        public void RefParameter(ref int parameter) {
            parameter = 42;
        }

        public void OutParameter(out int parameter) {
            parameter = 42;
        }

        public string GuidParameter(Guid guid) {
            return guid.ToString();
        }

        public string NullableGuidParameter(Guid? guid) {
            return guid?.ToString();
        }

        public void RandomListMethod(bool first = false, Guid? second = null, Guid? third = null, int[] last = null) {

        }

        public Parameter Parameter(string name, string value) {
            return new Parameter {
                Name = name,
                Value = value
            };
        }

        [Test, Parallelizable]
        public void CallMethodWithArrayParameters() {
            IScript script = parser.Parse("$test.methodwitharrayparameters(\"success\", [$test.parameter(\"n\", \"1\"),$test.parameter(\"m\", \"7\")])", new Variable("test", this));
            Assert.AreEqual("success:n=1,m=7", script.Execute());
        }

        [Test, Parallelizable]
        public void CallMethodWithParamsArray()
        {
            IScript script = parser.Parse("$test.methodwitharrayparameters(\"success\", $test.parameter(\"n\", \"1\"),$test.parameter(\"m\", \"7\"))", new Variable("test", this));
            Assert.AreEqual("success:n=1,m=7", script.Execute());
        }

        [Test, Parallelizable]
        public void CallParamsArrayWithoutArguments() {
            IScript script = parser.Parse("$test.methodwitharrayparameters(\"success\")", new Variable("test", this));
            Assert.AreEqual("success:", script.Execute());
        }

        [Test, Parallelizable]
        public void CallMethodWithDefaultParameters() {
            IScript script = parser.Parse("$test.methodwithdefaults(\"max\")", new Variable("test", this));
            Assert.AreEqual(" max ", script.Execute());
        }

        [Test, Parallelizable]
        public void ConvertGuidToStringOnCall() {
            IScript script = parser.Parse("$test.methodwithdefaults($guid)", new Variable("test", this), new Variable("guid", Guid.Empty));
            Assert.AreEqual($" {Guid.Empty} ", script.Execute());
        }

        [Test, Parallelizable]
        public void CallMethodSpecifyingDefaults() {
            IScript script = parser.Parse("$test.methodwithdefaults(\"max\", \"dr\")", new Variable("test", this));
            Assert.AreEqual("dr max ", script.Execute());
        }

        [Test, Parallelizable]
        public void CallMethodSpecifyingDefaultsAndParams() {
            IScript script = parser.Parse("$test.methodwithdefaults(\"max\", \"dr\", \"k.\", \"möllemann\")", new Variable("test", this));
            Assert.AreEqual("dr max k. möllemann", script.Execute());
        }

        [Test, Parallelizable]
        public void CallAmbigiousMethod() {
            IScript script = parser.Parse("$test.ambigious(50)", new Variable("test", this));
            Assert.AreEqual("int", script.Execute());
        }

        [Test, Parallelizable]
        public void CallAmbigiousMethodWithFloat()
        {
            IScript script = parser.Parse("$test.ambigious(50.3)", new Variable("test", this));
            Assert.AreEqual("string", script.Execute());
        }

        [Test, Parallelizable]
        public void CallInterfaceParameter()
        {
            IScript script = parser.Parse("$test.interfaceparameter($test.parameter(\"key\",\"value\"))", new Variable("test", this));
            Assert.AreEqual("parameter", script.Execute());
        }

        [Test, Parallelizable]
        public void CallEnumParameter()
        {
            IScript script = parser.Parse("$test.enumparameter(\"second\")", new Variable("test", this));
            Assert.AreEqual("enum", script.Execute());
        }

        [Test, Parallelizable]
        [TestCase("'a'")]
        [TestCase("7")]
        [TestCase("7u")]
        [TestCase("7l")]
        [TestCase("7ul")]
        [TestCase("7s")]
        [TestCase("7us")]
        public void CallAmbigiousMethodInt(string parameter)
        {
            IScript script = parser.Parse($"$test.ambigious({parameter})", new Variable("test", this));
            Assert.AreEqual("int", script.Execute());
        }

        [Test, Parallelizable]
        public void CallAmbigiousMethodByteArray() {
            IScript script = parser.Parse("$test.ambigious([1,2,3,4,5])", new Variable("test", this));
            Assert.AreEqual("bytearray", script.Execute());
        }

        [Test, Parallelizable]
        public void CallMethodWithRefParameter() {
            IScript script = parser.Parse(ScriptCode.Create(
                "$variable=0",
                "$test.refparameter(ref($variable))",
                "return($variable)"
            ), new Variable("test", this));

            Assert.AreEqual(42, script.Execute());
        }

        [Test, Parallelizable]
        public void CallMethodWithOutParameter()
        {
            IScript script = parser.Parse(ScriptCode.Create(
                "$variable=0",
                "$test.outparameter(ref($variable))",
                "return($variable)"
            ), new Variable("test", this));

            Assert.AreEqual(42, script.Execute());
        }

        [Test, Parallelizable]
        public void CallParamArrayUsingDictionaryConversion() {
            IScript script = parser.Parse(
                ScriptCode.Create(
                    "$test.methodwitharrayparameters(",
                    "  \"success\", ",
                    "  {",
                    "    \"name\": \"n\",",
                    "    \"value\": 1",
                    "  },",
                    "  {",
                    "    \"name\": \"m\",",
                    "    \"value\": 7",
                    "  }",
                    ")"),
                new Variable("test", this));
            Assert.AreEqual("success:n=1,m=7", script.Execute());
        }

        [Test, Parallelizable]
        public void CallSingleParameterUsingDictionaryConversion() {
            IScript script = parser.Parse(
                ScriptCode.Create(
                    "$test.methodwithsingleparameter(",
                    "  \"success\", ",
                    "  {",
                    "    \"name\": \"n\",",
                    "    \"value\": 42",
                    "  }",
                    ")"),
                new Variable("test", this));
            Assert.AreEqual("success:n=42", script.Execute());
        }

        [Test, Parallelizable]
        public void AutoConvertStringToGuid() {
            // 4cac6f34-ab34-48e5-bc5c-a0a23d282846

            IScript script = parser.Parse("$test.guidparameter(\"4cac6f34-ab34-48e5-bc5c-a0a23d282846\")", new Variable("test", this));
            Assert.AreEqual("4cac6f34-ab34-48e5-bc5c-a0a23d282846", script.Execute());
        }

        [Test, Parallelizable]
        public void AutoConvertStringToNullableGuid() {
            // 4cac6f34-ab34-48e5-bc5c-a0a23d282846

            IScript script = parser.Parse("$test.nullableguidparameter(\"4cac6f34-ab34-48e5-bc5c-a0a23d282846\")", new Variable("test", this));
            Assert.AreEqual("4cac6f34-ab34-48e5-bc5c-a0a23d282846", script.Execute());
        }

        [Test, Parallelizable]
        public void CallNullableMixedWithDefaults() {
            // 4cac6f34-ab34-48e5-bc5c-a0a23d282846

            IScript script = parser.Parse("$test.randomlistmethod(false, \"4cac6f34-ab34-48e5-bc5c-a0a23d282846\", null, [1,2])", new Variable("test", this));
            Assert.DoesNotThrow(()=>script.Execute());
        }
    }
}