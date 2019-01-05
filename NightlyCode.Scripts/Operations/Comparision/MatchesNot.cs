﻿using System.Text.RegularExpressions;
using NightlyCode.Scripting.Data;
using NightlyCode.Scripting.Errors;

namespace NightlyCode.Scripting.Operations.Comparision {

    /// <summary>
    /// determines whether a string matches a regex
    /// </summary>
    class MatchesNot : Comparator {

        /// <inheritdoc />
        protected override object Compare() {
            string pattern = Rhs.Execute() as string;
            if (pattern == null)
                throw new ScriptRuntimeException("Matching pattern must be a regex string");

            string value = Lhs.Execute()?.ToString();
            if (value == null)
                return false;

            return !Regex.IsMatch(value, pattern);
        }

        /// <inheritdoc />
        public override Operator Operator => Operator.NotMatches;

        /// <inheritdoc />
        public override string ToString() {
            return $"{Lhs} !~ {Rhs}";
        }
    }
}