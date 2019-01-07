﻿using NightlyCode.Scripting.Parser;
using NightlyCode.Scripting.Tokens;

namespace NightlyCode.Scripting.Control {

    /// <summary>
    /// a block of statements executed in sequence
    /// </summary>
    class StatementBlock : IScriptToken {
        readonly IVariableContext variablecontext;
        readonly IScriptToken[] statements;
        readonly bool methodblock;

        /// <summary>
        /// creates a new <see cref="StatementBlock"/>
        /// </summary>
        /// <param name="variablecontext">variable environment for this block</param>
        /// <param name="statements">statements in block</param>
        /// <param name="methodblock">determines whether this is the main block of a method</param>
        public StatementBlock(IScriptToken[] statements, IVariableContext variablecontext=null, bool methodblock=false) {
            this.variablecontext = variablecontext;
            this.statements = statements;
            this.methodblock = methodblock;
        }

        /// <inheritdoc />
        public object Execute() {
            object result;
            if (variablecontext != null) {
                
                using (variablecontext) {
                    result=ExecuteBlock();
                }
                variablecontext.Clear();
            }
            else result=ExecuteBlock();
            return result;
        }

        object ExecuteBlock() {
            object result = null;
            foreach (IScriptToken statement in statements)
            {
                result = statement.Execute();
                if (result is Return @return)
                {
                    if (methodblock)
                        return @return.Value?.Execute();
                    return @return;
                }

                if (result is Break)
                    return result;
            }

            return result;
        }

    }
}