﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NightlyCode.Scripting.Data;
using NightlyCode.Scripting.Errors;
using NightlyCode.Scripting.Operations;
using NightlyCode.Scripting.Parser;

namespace NightlyCode.Scripting.Tokens {

    /// <summary>
    /// calls a method in a script
    /// </summary>
    public class ScriptMethod : ScriptToken, IParameterContainer {
        readonly IExtensionProvider extensions;
        readonly IScriptToken hosttoken;
        readonly string methodname;
        readonly IScriptToken[] parameters;

        /// <summary>
        /// creates a new <see cref="ScriptMethod"/>
        /// </summary>
        /// <param name="extensions">pool containing known hosts</param>
        /// <param name="hosttoken">host of method to be called</param>
        /// <param name="methodname">name of method to call</param>
        /// <param name="parameters">parameters for method call</param>
        public ScriptMethod(IExtensionProvider extensions, IScriptToken hosttoken, string methodname, IScriptToken[] parameters) {
            this.extensions = extensions;
            this.hosttoken = hosttoken;
            this.methodname = methodname.ToLower();
            this.parameters = parameters;
        }

        /// <summary>
        /// host to call method on
        /// </summary>
        public IScriptToken Host => hosttoken;

        /// <summary>
        /// name of method to call
        /// </summary>
        public string MethodName => methodname;

        /// <inheritdoc />
        public IEnumerable<IScriptToken> Parameters => parameters;

        /// <inheritdoc />
        public override string Literal => $".{methodname}()";

        /// <inheritdoc />
        protected override object ExecuteToken(ScriptContext context) {
            object host = hosttoken.Execute(context);
            if (host == null)
                throw new ScriptExecutionException($"'{hosttoken}' results in null");

            if (host is IExternalMethod externmethod && MethodName.ToLower()=="invoke")
                return externmethod.Invoke(context.Arguments, Parameters.Select(a => a.Execute(context)).ToArray());

            MethodInfo[] methods = host.GetType().GetMethods().Where(m => m.Name.ToLower() == methodname && MethodOperations.MatchesParameterCount(m, parameters)).ToArray();

            object[] parametervalues = parameters.Select(p => p.Execute(context)).ToArray();

            List<ReferenceParameter> references=new List<ReferenceParameter>();
            for (int i = 0; i < parameters.Length; ++i) {
                if (!(parameters[i] is Reference r))
                    continue;
                references.Add(new ReferenceParameter(i, r));
            }

            Tuple<MethodInfo, int>[] evaluation = methods.Select(m => MethodOperations.GetMethodMatchValue(m, parametervalues)).Where(e=>e.Item2>=0).OrderBy(m => m.Item2).ToArray();
            if (evaluation.Length > 0)
                return MethodOperations.CallMethod(host, evaluation[0].Item1, parametervalues, context, references);

            if (extensions != null) {
                Type extensionbase = host.GetType();
                while (extensionbase != null) {
                    methods = extensions.GetExtensions(extensionbase).Where(m => m.Name.ToLower() == methodname && MethodOperations.MatchesParameterCount(m, parameters, true)).ToArray();
                    evaluation = methods.Select(m => MethodOperations.GetMethodMatchValue(m, parametervalues, true)).OrderBy(m => m.Item2).ToArray();
                    if (evaluation.Length > 0)
                        return MethodOperations.CallMethod(host, evaluation[0].Item1, parametervalues, context, references, true);
                    

                    if (extensionbase == typeof(object))
                        break;
                    extensionbase = extensionbase.BaseType;
                }


                foreach (Type interfacetype in host.GetType().GetInterfaces()) {
                    methods = extensions.GetExtensions(interfacetype).Where(m => m.Name.ToLower() == methodname && MethodOperations.MatchesParameterCount(m, parameters, true)).ToArray();
                    evaluation = methods.Select(m => MethodOperations.GetMethodMatchValue(m, parametervalues, true)).OrderBy(m => m.Item2).ToArray();
                    if (evaluation.Length > 0)
                        return MethodOperations.CallMethod(host, evaluation[0].Item1, parametervalues, context, references, true);
                }
            }

            throw new ScriptRuntimeException($"Method '{methodname}' matching the parameters '({string.Join(",", parametervalues)})' not found on type {host.GetType().Name}");
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{hosttoken}.{methodname}({string.Join(",", parameters.Select(p => p.ToString()))})";
        }
    }
}