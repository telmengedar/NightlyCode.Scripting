﻿using NightlyCode.Scripting.Data;
using NightlyCode.Scripting.Errors;
using NightlyCode.Scripting.Tokens;

namespace NightlyCode.Scripting.Operations.Assign {

    /// <summary>
    /// assignment in a script
    /// </summary>
    class Assignment : IBinaryToken, IOperator, IAssignableToken {
        IAssignableToken lhs;

        /// <inheritdoc />
        public object Execute() {
            return lhs.Assign(Rhs);
        }

        /// <inheritdoc />
        public object Assign(IScriptToken token) {
            if (Rhs is IAssignableToken assignable)
                return assignable.Assign(token);
            throw new ScriptRuntimeException("Can't assign value to non assignable token");
        }

        /// <inheritdoc />
        public IScriptToken Lhs {
            get => lhs;
            set {
                lhs=value as IAssignableToken;
                if (lhs == null)
                    throw new ScriptRuntimeException("Left hand side of assignment has to be an assignable token");
            }
        }

        /// <inheritdoc />
        public IScriptToken Rhs { get; set; }

        /// <inheritdoc />
        public Operator Operator => Operator.Assignment;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Lhs} = {Rhs}";
        }
    }
}