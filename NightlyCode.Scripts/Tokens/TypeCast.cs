﻿using System;
using NightlyCode.Scripting.Extern;
using NightlyCode.Scripting.Parser;

namespace NightlyCode.Scripting.Tokens {

    /// <summary>
    /// changes the type of an expression result
    /// </summary>
    public class TypeCast : ScriptToken {
        readonly Type targettype;
        readonly IScriptToken token;

        /// <summary>
        /// creates a new <see cref="TypeCast"/>
        /// </summary>
        /// <param name="targettype">target type</param>
        /// <param name="token">token resulting in value to cast</param>
        public TypeCast(Type targettype, IScriptToken token) {
            this.targettype = targettype;
            this.token = token;
        }

        /// <summary>
        /// type to cast argument to
        /// </summary>
        public Type TargetType => targettype;

        /// <summary>
        /// argument to cast
        /// </summary>
        public IScriptToken Argument => token;

        /// <inheritdoc />
        protected override object ExecuteToken(IVariableContext variables, IVariableProvider arguments) {
            return Converter.Convert(token.Execute(variables, arguments), targettype);
        }
    }
}