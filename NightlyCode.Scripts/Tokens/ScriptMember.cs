﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NightlyCode.Scripting.Errors;
using NightlyCode.Scripting.Extern;
using NightlyCode.Scripting.Operations;

namespace NightlyCode.Scripting.Tokens {

    /// <summary>
    /// reads a value from a host member
    /// </summary>
    public class ScriptMember : AssignableToken, ICodePositionToken {
        readonly IScriptToken hosttoken;
        readonly string membername;

        /// <summary>
        /// creates a new <see cref="ScriptMember"/>
        /// </summary>
        /// <param name="host">host of member</param>
        /// <param name="membername">name of member to read</param>
        internal ScriptMember(IScriptToken host, string membername) {
            hosttoken = host;
            this.membername = membername;
        }

        /// <summary>
        /// instance of which member is served
        /// </summary>
        public IScriptToken Host => hosttoken;

        /// <summary>
        /// name of member
        /// </summary>
        public string Member => membername;

        /// <inheritdoc />
        public override string Literal => $".{membername}";

        /// <inheritdoc />
        protected override object ExecuteToken(ScriptContext context) {
            object host = hosttoken.Execute(context);

            if (host is Dictionary<object, object> dictionary)
                return dictionary[Member];

            string member = membername.ToLower();
            PropertyInfo property = host.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == member);
            if(property != null) {
                try {
                    return property.GetValue(host);
                }
                catch(Exception e) {
                    throw new ScriptRuntimeException("Unable to read property", null, e);
                }
            }


            FieldInfo fieldinfo = host.GetType().GetFields().FirstOrDefault(f => f.Name.ToLower() == member);
            if (fieldinfo == null)
                throw new ScriptRuntimeException($"A member with the name of {membername} was not found in type {host.GetType().Name}", this);

            try {
                return fieldinfo.GetValue(host);
            }
            catch(Exception e) {
                throw new ScriptRuntimeException("Unable to read field", null, e);
            }
        }

        object SetProperty(object host, PropertyInfo property, IScriptToken valuetoken, ScriptContext context)
        {
            object targetvalue = Converter.Convert(valuetoken.Execute(context), property.PropertyType);
            try
            {
                property.SetValue(host, targetvalue, null);
            }
            catch (Exception e)
            {
                throw new ScriptRuntimeException("Unable to set property", null, e);
            }

            return targetvalue;
        }

        object SetField(object host, FieldInfo fieldinfo, IScriptToken valuetoken, ScriptContext context)
        {
            object targetvalue = Converter.Convert(valuetoken.Execute(context), fieldinfo.FieldType);
            try
            {
                fieldinfo.SetValue(host, targetvalue);
            }
            catch (Exception e)
            {
                throw new ScriptRuntimeException("Unable to set field", null, e);
            }

            return targetvalue;
        }

        /// <inheritdoc />
        protected override object AssignToken(IScriptToken token, ScriptContext context) {
            object host = hosttoken.Execute(context);
            if (host is Dictionary<object, object> dictionary)
                return dictionary[Member] = token.Execute(context);

            string member = membername.ToLower();
            PropertyInfo property = host.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == member);
            if (property != null)
                return SetProperty(host, property, token, context);

            FieldInfo fieldinfo = host.GetType().GetFields().FirstOrDefault(f => f.Name.ToLower() == member);
            if (fieldinfo == null)
                throw new ScriptRuntimeException($"A member with the name of {membername} was not found in type {host.GetType().Name}", this);

            return SetField(host, fieldinfo, token, context);
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{hosttoken}.{membername}";
        }
    }
}