﻿using NightlyCode.Scripting.Errors;
using NightlyCode.Scripting.Extensions;

namespace NightlyCode.Scripting.Control {

    /// <summary>
    /// executes a statement block while a condition is met
    /// </summary>
    public class While : IControlToken {
        readonly IScriptToken condition;

        /// <summary>
        /// creates a new <see cref="While"/>
        /// </summary>
        /// <param name="parameters">parameters containing condition to check</param>
        public While(IScriptToken[] parameters) {
            if (parameters.Length != 1)
                throw new ScriptParserException("While needs exactly one condition as parameter");
            condition = parameters[0];
        }

        /// <inheritdoc />
        public object Execute() {
            while (condition.Execute().ToBoolean()) {
                object value=Body.Execute();
                if (value is Return)
                    return value;
                if (value is Break)
                    return null;
            }

            return null;
        }

        /// <inheritdoc />
        public IScriptToken Body { get; set; }
    }
}