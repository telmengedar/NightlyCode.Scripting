﻿namespace NightlyCode.Scripting.Data {

    /// <summary>
    /// operator type
    /// </summary>
    /// <remarks>
    /// the order in this enumeration is used for operator priority
    /// </remarks>
    public enum Operator {
        Not,
        Negate,
        Complement,
        Postcrement,
        Precrement,
        Division,
        Multiplication,
        Modulo,
        Subtraction,
        Addition,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        Equal,
        NotEqual,
        Matches,
        NotMatches,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        ShiftLeft,
        ShiftRight,
        LogicAnd,
        LogicOr,
        LogicXor,
        Assignment,
        AddAssign,
        SubAssign,
        DivAssign,
        MulAssign,
        ModAssign,
        ShlAssign,
        ShrAssign,
        AndAssign,
        OrAssign,
        XorAssign
    }
}