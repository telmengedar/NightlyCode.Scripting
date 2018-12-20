﻿using System;
using System.Linq;
using System.Reflection;
using NightlyCode.Core.Conversion;

namespace NightlyCode.Scripting.Tokens {

    /// <summary>
    /// reads a value from a host member
    /// </summary>
    public class ScriptMember : IScriptToken {
        readonly IScriptToken hosttoken;
        readonly string membername;

        /// <summary>
        /// creates a new <see cref="ScriptMember"/>
        /// </summary>
        /// <param name="host">host of member</param>
        /// <param name="membername">name of member to read</param>
        public ScriptMember(IScriptToken host, string membername) {
            hosttoken = host;
            this.membername = membername.ToLower();
        }

        /// <inheritdoc />
        public object Execute() {
            object host = hosttoken.Execute();
            PropertyInfo property = host.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == membername);
            if(property != null) {
                try {
                    return property.GetValue(host);
                }
                catch(Exception e) {
                    throw new ScriptException("Unable to read property", null, e);
                }
            }


            FieldInfo fieldinfo = host.GetType().GetFields().FirstOrDefault(f => f.Name.ToLower() == membername);
            if (fieldinfo == null)
                throw new ScriptException($"A member with the name of {membername} was not found in type {host.GetType().Name}");

            try {
                return fieldinfo.GetValue(host);
            }
            catch(Exception e) {
                throw new ScriptException("Unable to read field", null, e);
            }
        }

        object SetProperty(object host, PropertyInfo property, IScriptToken valuetoken)
        {
            object targetvalue = Converter.Convert(valuetoken.Execute(), property.PropertyType);
            try
            {
                property.SetValue(host, targetvalue, null);
            }
            catch (Exception e)
            {
                throw new ScriptException("Unable to set property", null, e);
            }

            return targetvalue;
        }

        object SetField(object host, FieldInfo fieldinfo, IScriptToken valuetoken)
        {
            object targetvalue = Converter.Convert(valuetoken.Execute(), fieldinfo.FieldType);
            try
            {
                fieldinfo.SetValue(host, targetvalue);
            }
            catch (Exception e)
            {
                throw new ScriptException("Unable to set field", null, e);
            }

            return targetvalue;
        }

        public object Assign(IScriptToken token) {
            object host = hosttoken.Execute();
            PropertyInfo property = host.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == membername);
            if (property != null)
                return SetProperty(host, property, token);

            FieldInfo fieldinfo = host.GetType().GetFields().FirstOrDefault(f => f.Name.ToLower() == membername);
            if (fieldinfo == null)
                throw new ScriptException($"A member with the name of {membername} was not found in type {host.GetType().Name}");

            return SetField(host, fieldinfo, token);
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{hosttoken}.{membername}";
        }
    }
}