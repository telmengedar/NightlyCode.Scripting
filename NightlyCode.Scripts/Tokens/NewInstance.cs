﻿using System;
using System.Collections.Generic;
using NightlyCode.Scripting.Errors;
using NightlyCode.Scripting.Parser;

namespace NightlyCode.Scripting.Tokens {

    /// <summary>
    /// creates a new instance of a type
    /// </summary>
    public class NewInstance : ScriptToken, IParameterContainer {
        readonly ITypeInstanceProvider provider;
        readonly IScriptToken[] parameters;

        internal NewInstance(string typename, Type type, ITypeInstanceProvider provider, IScriptToken[] parameters) {
            TypeName = typename;
            Type = type;
            this.provider = provider;
            this.parameters = parameters;
        }

        /// <summary>
        /// name of type in script
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// type to be created (just an indicator for reflection)
        /// </summary>
        /// <remarks>
        /// when in doubt use typeof(object)
        /// </remarks>
        public Type Type { get; }

        /// <inheritdoc />
        public override string Literal => "new";

        /// <inheritdoc />
        protected override object ExecuteToken(ScriptContext context) {
            try {
                return provider.Create(parameters, context);
            }
            catch (ScriptRuntimeException e) {
                throw new ScriptRuntimeException(e.Message, this, e.InnerException);
            }
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"new {provider}({string.Join<IScriptToken>(", ", parameters)})";
        }

        /// <inheritdoc />
        public IEnumerable<IScriptToken> Parameters => parameters;
    }
}