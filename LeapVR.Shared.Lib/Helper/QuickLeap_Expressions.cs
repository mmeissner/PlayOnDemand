#region Licence
/****************************************************************
 *  Filename: QuickLeap_Expressions.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LeapVR.Shared.Lib.Classes;

namespace LeapVR.Shared.Lib.Helper
{
    public static partial class QuickLeap
    {
        public static Expression<T> ExpressionCompose<T>(Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> ExpressionAnd<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return ExpressionCompose(first, second, Expression.And);
        }

        public static Expression<Func<T, bool>> ExpressionOr<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return ExpressionCompose(first, second, Expression.Or);
        }

        public static MemberInfo GetMemberInfo<T>(Expression<Func<T>> expression)
        {
            var memberExp = expression.Body as MemberExpression;
            var unaryExp = expression.Body as UnaryExpression;
            var body = (MemberExpression)(unaryExp != null ? unaryExp.Operand : memberExp);
            return body.Member;
        }

        public static string GetFieldPropertyName<T>(Expression<Func<T>> expression)
        {
            return GetMemberInfo(expression).Name;
        }
    }
}
