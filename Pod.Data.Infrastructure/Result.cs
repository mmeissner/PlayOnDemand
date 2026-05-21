#region Licence
/****************************************************************
 *  Filename: Result.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pod.Enums;

namespace Pod.Data.Infrastructure
{
    /// <summary>
    /// Provides Validation, Error Information and a return Value
    /// </summary>
    /// <typeparam name="T">The Return Value</typeparam>
    public class Result<T> : Result, IResult<T>
    {
        /// <summary>
        /// Adds an Return Value to the result
        /// </summary>
        /// <param name="retVal">The Return Value</param>
        /// <returns>Self</returns>
        public Result<T> Add(T retVal)
        {
            ReturnValue = retVal;
            return this;
        }

        /// <summary>
        /// Adds Error Information to this IResult if any present
        /// </summary>
        /// <param name="errorResult">The result to merge the error information with</param>
        /// <returns>Self</returns>
        public new Result<T> Add(IResult errorResult)
        {
            base.Add(errorResult);
            return this;
        }

        /// <summary>
        /// Adds an error to this result
        /// </summary>
        /// <param name="errorMessages">The error message</param>
        /// <param name="error">The Error Code</param>
        /// <returns>Self</returns>
        public new Result<T> Add(string errorMessages, UserError error)
        {
            base.Add(errorMessages, error);
            return this;
        }
        public T ReturnValue { get; private set; }
    }

    /// <summary>
    /// Provides Validation, Error Information
    /// </summary>
    public class Result : IResult
    {
        private readonly Dictionary<UserError,List<string>> _dictErrors = new Dictionary<UserError, List<string>>();
        
        /// <summary>
        /// Validates for true
        /// </summary>
        /// <param name="arg">The value to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when no error, false in case of error</returns>
        public bool ArgTrue(bool arg, string paramName, UserError error = UserError.InternalError)
        {
            if(!arg)
            {
                Add(error, $"Argument with name {paramName} must be true");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates for false
        /// </summary>
        /// <param name="arg">The value to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when no error, false in case of error</returns>
        public bool ArgFalse(bool arg, string paramName, UserError error = UserError.InternalError)
        {
            if(arg)
            {
                Add(error, $"Argument with name {paramName} must be false");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a string is not NullOrWhitespace
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when no error, false in case of error</returns>
        public bool ArgNotNullOrWhitespace(string value, string paramName, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                Add(error, $"Argument can not be null or Whitespace: {paramName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a object reference is not null
        /// </summary>
        /// <param name="arg">The reference object to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when no error, false in case of error</returns>
        public bool ArgNotNull(object arg, string paramName, UserError error = UserError.InternalError)
        {
            if(arg == null)
            {
                Add(error, $"Argument can not be null: {paramName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a Guid is not empty
        /// </summary>
        /// <param name="arg">The Guid for validation</param>
        /// <param name="paramName">The name of the parameter or variable</param>
        /// <param name="error">The Error to set in case its empty</param>
        /// <returns>true if not empty, otherwise false</returns>
        public bool ArgNotEmpty(Guid arg, string paramName, UserError error = UserError.InternalError)
        {
            if (arg == Guid.Empty)
            {
                Add(error, $"Guid can not be empty: {paramName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a nullable Guid is not empty
        /// </summary>
        /// <param name="arg">The nullable Guid for validation</param>
        /// <param name="paramName">The name of the parameter or variable</param>
        /// <param name="error">The Error to set in case its empty</param>
        /// <returns>true if not empty, otherwise false</returns>
        public bool ArgNotNullOrEmpty(Guid? arg, string paramName, UserError error = UserError.InternalError)
        {
            if (arg == null || arg == Guid.Empty)
            {
                Add(error, $"Guid can not be null or empty: {paramName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a IEnumerable is null or does not contain any entries
        /// </summary>
        /// <param name="enumerable">The enumerable for validation</param>
        /// <param name="paramName">The name of the parameter or variable</param>
        /// <param name="error">The Error to set in case its null or empty</param>
        /// <returns>true if not null or empty, otherwise false</returns>
        public bool ArgNotNullOrEmpty<T>(IEnumerable<T> enumerable, string paramName, UserError error = UserError.InternalError)
        {
            if (enumerable == null || !enumerable.Any())
            {
                Add(error, $"The Enumerable with name {paramName} can not be null or have no contents");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates if a object reference is null
        /// </summary>
        /// <param name="arg">The reference object to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when no error, false in case of error</returns>
        public bool ArgNull(object arg, string paramName, UserError error = UserError.InternalError)
        {
            if(arg != null)
            {
                Add(error, $"Argument must be null: {paramName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if two objects are equal or both are null
        /// </summary>
        /// <param name="arg">The reference object to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="equal">The reference object to compare to</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when equal, false if not</returns>
        public bool ArgEqual(object arg, string paramName, object equal, UserError error = UserError.InternalError)
        {
            if(arg == null && equal == null)
            {
                return true;
            }

            if(arg != null && equal == null || arg == null || !arg.Equals(equal))
            {
                Add(error, $"Argument with name {paramName} can not be equal to {equal}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if two objects are not equal or one of them not null
        /// </summary>
        /// <param name="arg">The reference object to validate</param>
        /// <param name="paramName">The name of the value</param>
        /// <param name="notEqual">The reference object to compare to</param>
        /// <param name="error">The Error Code</param>
        /// <returns>True when equal, false if not</returns>
        public bool ArgNotEqual(
                object arg, string paramName, object notEqual, UserError error = UserError.InternalError)
        {
            if(arg != null && notEqual == null || arg == null && notEqual != null)
            {
                return true;
            }

            if(arg == null || arg.Equals(notEqual))
            {
                Add(error, $"Argument with name {paramName} can not be equal to {notEqual}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates two enums if they are not equal
        /// </summary>
        /// <param name="enumType">The type of the enums to compare</param>
        /// <param name="enumValue">The first enum value</param>
        /// <param name="notEnumValue">The second enum value</param>
        /// <param name="paramName">The parameter/variable name of the first enum</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if not equal, otherwise false</returns>
        public bool ArgNotEnum(
                Type enumType, object enumValue, object notEnumValue, string paramName,
                UserError error = UserError.InternalError)
        {
            if(enumValue.Equals(notEnumValue))
            {
                Add(
                        error,
                        $"Enum Argument with name {paramName} and type {enumType.FullName} can not be of value {enumType.GetEnumName(enumValue)}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates two enums if they are equal
        /// </summary>
        /// <param name="enumType">The type of the enums to compare</param>
        /// <param name="enumValue">The first enum value</param>
        /// <param name="needEnumValue">The second enum value</param>
        /// <param name="paramName">The parameter/variable name of the first enum</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if equal, otherwise false</returns>
        public bool ArgIsEnum(
                Type enumType, object enumValue, object needEnumValue, string paramName,
                UserError error = UserError.InternalError)
        {
            if(!enumValue.Equals(needEnumValue))
            {
                Add(
                        error,
                        $"Enum Argument with name {paramName} and type {enumType.FullName} must be of value {enumType.GetEnumName(enumValue)}");

                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Validates if an int is not lower then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower, otherwise false</returns>
        public bool ArgNotLowerThen(
                int value, string paramName, int conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value < conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be lower then parameter {paramCondition} with value {conditionValue}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates if an decimal is not lower then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower, otherwise false</returns>
        public bool ArgNotLowerThen(
                decimal value, string paramName, decimal conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value < conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be lower then parameter {paramCondition} with value {conditionValue}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if an long is not lower then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public bool ArgNotLowerThen(
                long value, string paramName, long conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value < conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be lower then parameter {paramCondition} with value {conditionValue}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if an timespan is not lower then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public bool ArgNotLowerThen(
                TimeSpan value, string paramName, TimeSpan conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if (value < conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be lower then parameter {paramCondition} with value {conditionValue}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if an double is not lower or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public bool ArgNotLowerOrEqualThen(
                double value, string paramName, double conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value <= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be after {conditionValue} of {paramCondition}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if an int is not lower or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public void ArgNotLowerOrEqualThen(
                int value, string paramName, int conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value <= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be after {conditionValue} of {paramCondition}");
            }
        }

        /// <summary>
        /// Validates if an decimal is not lower or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public void ArgNotLowerOrEqualThen(
                decimal value, string paramName, decimal conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value <= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be after {conditionValue} of {paramCondition}");
            }
        }

        /// <summary>
        /// Validates if an long is not lower or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public bool ArgNotLowerOrEqualThen(
                long value, string paramName, long conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value <= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be after {conditionValue} of {paramCondition}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if an long is not lower then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not lower or equal, otherwise false</returns>
        public void ArgNotLowerOrEqualThen(
                TimeSpan value, string paramName, TimeSpan conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value <= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be after {conditionValue} of {paramCondition}");
            }
        }

        /// <summary>
        /// Validates if an int is not higher then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not higher, otherwise false</returns>
        public void ArgNotHigherThen(
                int value, string paramName, int conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if (value > conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be higher then {paramCondition} with value {conditionValue}");
            }
        }

        /// <summary>
        /// Validates if an timespan is not higher then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the threshold</param>
        /// <param name="error">The error to set if the base is lower then the threshold</param>
        /// <returns>true if not higher, otherwise false</returns>
        public bool ArgNotHigherThen(
                TimeSpan value, string paramName, TimeSpan conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if (value > conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be higher then {paramCondition} with value {conditionValue}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a string for a length
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="minChars">The value to compare length with</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string is not shorter and not null or whitespace, otherwise false</returns>
        public bool StringNotShorterThen(
                string value, string valueName, int minChars, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || value.Length < minChars)
            {
                Add(
                        error,
                        $"Value of parameter {valueName} is {value} and does not have a minimum of {minChars} characters");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a string for a length
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="maxChars">The value to compare length with</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string is not longer and not null or whitespace, otherwise false</returns>
        public bool StringNotLongerThen(
                string value, string valueName, int maxChars, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || value.Length > maxChars)
            {
                Add(
                        error,
                        $"Value of parameter {valueName} is {value} and is null/whitespace or has more then the allowed {maxChars} characters ");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a string contains uppercase characters
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string contains uppercase chars and is not null or whitespace, otherwise false</returns>
        public bool StringMustContainUpperCase(
                string value, string valueName, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || !value.Any(IsUpper))
            {
                Add(error, $"Value of parameter {valueName} is {value} and is null/whitespace or does not contain any upper case characters");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a string contains lowercase characters
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string contains lowercase chars and is not null or whitespace, otherwise false</returns>
        public bool StringMustContainLowerCase(
                string value, string valueName, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || !value.Any(IsLower))
            {
                Add(error, $"Value of parameter {valueName} is {value} and is null/whitespace or does not contain any lower case characters");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates if a string contains special characters
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string contains special chars and is not null or whitespace, otherwise false</returns>
        public void StringMustContainSpecialChars(
                string value, string valueName, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || value.All(IsLetterOrDigit))
            {
                Add(error, $"Value of parameter {valueName} is {value} and is null/whitespace or does not contain special characters");
            }
        }

        /// <summary>
        /// Validates if a string contains numbers
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string contains numbers and is not null or whitespace, otherwise false</returns>
        public void StringMustContainNumbers(string value, string valueName, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || !value.Any(IsDigit))
            {
                Add(error, $"Value of parameter {valueName} is {value} and does not contain any digits");
            }
        }

        /// <summary>
        /// Validates if a string contains a certain amount of unique characters
        /// Does not count whitespace or null strings as valid
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <param name="valueName">The parameter/variable name of the string</param>
        /// <param name="uniqueChars">The amount of unique characters required</param>
        /// <param name="error">The error type to set</param>
        /// <returns>true if string contains enough unique characters and is not null or whitespace, otherwise false</returns>
        public void StringMustUniqueChars(
                string value, string valueName, int uniqueChars, UserError error = UserError.InternalError)
        {
            if(string.IsNullOrWhiteSpace(value) || uniqueChars >= 1 && value.Distinct().Count() < uniqueChars)
            {
                Add(
                        error,
                        $"Value of parameter {valueName} is {value} and does not contain {uniqueChars} unique characters");
            }
        }

        /// <summary>
        /// Compares if an datetime is earlier then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the condition</param>
        /// <param name="error">The error to set if the base is earlier then the conditional value</param>
        /// <returns>true if not before, otherwise false</returns>
        public bool ArgNotBefore(
                DateTime value, string paramName, DateTime conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value < conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be before param {paramCondition} with value {conditionValue}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compares if an datetime is earlier or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the condition</param>
        /// <param name="error">The error to set if the base is earlier or equal then the conditional value</param>
        /// <returns>true if not before or equal, otherwise false</returns>
        public bool ArgNotBeforeOrEqualThen(
                DateTime value, string paramName, DateTime conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value <= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be before {paramCondition} wit value {conditionValue}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares if an datetime is after or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="conditionValue">The value to compare the base value to</param>
        /// <param name="paramCondition">The parameter/argument name of the value that sets the condition</param>
        /// <param name="error">The error to set if the base is after or equal then the conditional value</param>
        /// <returns>true if not after or equal, otherwise false</returns>
        public bool ArgNotAfterOrEqualThen(
                DateTime value, string paramName, DateTime conditionValue, string paramCondition,
                UserError error = UserError.InternalError)
        {
            if(value >= conditionValue)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but can not be after {paramCondition} wit value {conditionValue}");
                return false;
            }
            return true;
        }


        /// <summary>
        /// Compares if an value is in an certain range is after or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="from">The lower bound</param>
        /// <param name="to">the upper bound</param>
        /// <param name="error">The error to set if the base value is out of range</param>
        /// <returns>true if in range, otherwise false</returns>
        public bool ArgOutOfRange(
                int value, string paramName, int from, int to, UserError error = UserError.InternalError)
        {
            if(value > to || value < from)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but its value is outside of the allowed range from {from} to {to}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares if an value is in an certain range is after or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="from">The lower bound</param>
        /// <param name="to">the upper bound</param>
        /// <param name="error">The error to set if the base value is out of range</param>
        /// <returns>true if in range, otherwise false</returns>
        public bool ArgOutOfRange(
                decimal value, string paramName, decimal from, decimal to, UserError error = UserError.InternalError)
        {
            if(value > to || value < from)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but its value is outside of the allowed range from {from} to {to}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares if an value is in an certain range is after or equal then another
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="paramName">the base values parameter/variable name</param>
        /// <param name="from">The lower bound</param>
        /// <param name="to">the upper bound</param>
        /// <param name="error">The error to set if the base value is out of range</param>
        /// <returns>true if in range, otherwise false</returns>
        public bool ArgOutOfRange(
                long value, string paramName, long from, long to, UserError error = UserError.InternalError)
        {
            if(value > to || value < from)
            {
                Add(
                        error,
                        $"Argument value of parameter {paramName} is {value}, but its value is outside of the allowed range from {from} to {to}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an reference type object is set or null
        /// </summary>
        /// <param name="value">the reference type object</param>
        /// <param name="propertyName">the objects parameter/variable name</param>
        /// <param name="error">The error to set if the reference is null</param>
        /// <returns>true if the reference is set, otherwise false</returns>
        public bool RefNotNull(object value, string propertyName, UserError error = UserError.InternalError)
        {
            if(value == null)
            {
                Add(error, $"Reference can not be null: {propertyName}");

                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if an reference type object in an Db Entity is set
        /// </summary>
        /// <param name="value">the property/field the reference type object should be set</param>
        /// <param name="valueId">the objects primary key reference</param>
        /// <param name="refName">the values parameter/variable name</param>
        /// <param name="error">The error to set if the reference is not set</param>
        /// <returns>true if the reference is set, otherwise false</returns>
        public bool RefNotNull(object value, long? valueId, string refName, UserError error = UserError.InternalError)
        {
            if(value != null || valueId != null)
            {
                Add(error, $"An Reference already exist for Property {refName}!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an reference type object in an Db Entity is set
        /// </summary>
        /// <param name="value">the property/field the reference type object should be set</param>
        /// <param name="valueId">the objects primary key reference</param>
        /// <param name="refName">the values parameter/variable name</param>
        /// <param name="error">The error to set if the reference is not set</param>
        /// <returns>true if the reference is set, otherwise false</returns>
        public bool RefNotNull(object value, Guid? valueId, string refName, UserError error = UserError.InternalError)
        {
            if (value != null || valueId != null)
            {
                Add(error, $"An Reference already exist for Property {refName}!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if an reference type object in an Db Entity is set in case it should exist
        /// If the primary key reference is set to a valid value, then the reference type should also be set
        /// </summary>
        /// <param name="value">the property/field the reference type object should be set</param>
        /// <param name="valueId">the objects primary key reference</param>
        /// <param name="refName">the values parameter/variable name</param>
        /// <param name="error">The error to set if the reference is not set</param>
        /// <returns>true if the reference is set, otherwise false</returns>
        public bool RefNotNullIfExist(
                object value, long? valueId, string refName, UserError error = UserError.InternalError)
        {
            if(valueId.HasValue && value == null)
            {
                Add(
                        error,
                        $"An ReferenceId is {valueId} but the reference object with property name {refName} is not set!");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an reference type object in an Db Entity is set in case it should exist
        /// If the primary key reference is set to a valid value, then the reference type should also be set
        /// </summary>
        /// <param name="value">the property/field the reference type object should be set</param>
        /// <param name="valueId">the objects primary key reference</param>
        /// <param name="refName">the values parameter/variable name</param>
        /// <param name="error">The error to set if the reference is not set</param>
        /// <returns>true if the reference is set, otherwise false</returns>
        public bool RefNotNullIfExist(
                object value, Guid? valueId, string refName, UserError error = UserError.InternalError)
        {
            if (valueId.HasValue && value == null)
            {
                Add(
                        error,
                        $"An ReferenceId is {valueId} but the reference object with property name {refName} is not set!");

                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Checks if a Db key Guid is not null and not empty
        /// </summary>
        /// <param name="id">The key</param>
        /// <param name="idName">The key name</param>
        /// <param name="error">The error to set if its null</param>
        /// <returns>true if not null or empty otherwise false</returns>
        public bool ValueIdValid(Guid? id, string idName, UserError error = UserError.InternalError)
        {
            if (id == null || id.Value== Guid.Empty)
            {
                Add(error, $"Id of {idName}, must be valid and can not be null or empty");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a Db key Guid is not empty
        /// </summary>
        /// <param name="id">The key</param>
        /// <param name="idName">The key name</param>
        /// <param name="error">The error to set if its null</param>
        /// <returns>true if not empty otherwise false</returns>
        public bool ValueIdValid(Guid id, string idName, UserError error = UserError.InternalError)
        {
            if (id == Guid.Empty)
            {
                Add(error, $"Id of {idName}, must be valid and can not be empty");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets an error due to an invalid type reference value
        /// </summary>
        /// <param name="value">The value that cause the error</param>
        /// <param name="valueName">The value parameter/argument name</param>
        /// <param name="message">The message to set</param>
        /// <param name="error">The error to set</param>
        public void InvalidOperation(
                object value, string valueName, string message, UserError error = UserError.InternalError)
        {
            Add(error, $"Invalid Operation including value:{value}, valueName: {valueName}. {message}");
        }

        /// <summary>
        /// Checks if two objects are not equal
        /// Both are also considered equal if they are both null
        /// </summary>
        /// <param name="arg">The base value</param>
        /// <param name="paramName">The parameter/argument name of the base value</param>
        /// <param name="notEqual">The value to compare to</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if not equal, otherwise false</returns>
        public bool ValueNotEqual(
                object arg, string paramName, object notEqual, UserError error = UserError.InternalError)
        {
            if(arg != null && notEqual == null || arg == null && notEqual != null) return true;
            if(arg == null || arg.Equals(notEqual))
            {
                Add(error, $"Value with name {paramName} can not be equal to {notEqual}");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Checks if two objects are equal
        /// Both are also considered equal if they are both null
        /// </summary>
        /// <param name="arg">The base value</param>
        /// <param name="paramName">The parameter/argument name of the base value</param>
        /// <param name="equals">The value to compare to</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if equal, otherwise false</returns>
        public bool ValueEqual(object arg, string paramName, object equals, UserError error = UserError.InternalError)
        {
            if(arg == null && equals == null) return true;
            if(arg != null && equals == null || arg == null || !arg.Equals(equals))
            {
                Add(error, $"Value with name {paramName} can must equal to {equals}");

                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Checks if the bool is true
        /// </summary>
        /// <param name="arg">The value</param>
        /// <param name="paramName">The parameter/argument name of the base value</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if bool is true, otherwise false</returns>
        public bool ValueTrue(bool arg, string paramName, UserError error = UserError.InternalError)
        {
            if(!arg)
            {
                Add(error, $"Value with name {paramName} must be true");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the bool is false
        /// </summary>
        /// <param name="arg">The value</param>
        /// <param name="paramName">The parameter/argument name of the base value</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if bool is false, otherwise false</returns>
        public bool ValueFalse(bool arg, string paramName, UserError error = UserError.InternalError)
        {
            if(arg)
            {
                Add(error, $"Value with name {paramName} must be false");

                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Checks if the value is null
        /// </summary>
        /// <param name="arg">The value to check</param>
        /// <param name="paramName">The name of the parameter/argument of the value</param>
        /// <param name="error">The error to set</param>
        /// <returns>true if not null, otherwise false</returns>
        public bool ValueNotNull(object arg, string paramName, UserError error = UserError.InternalError)
        {
            if(arg == null)
            {
                Add(error, $"Value with name {paramName} can not be null");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the result contains any error
        /// </summary>
        /// <returns>true if there is no error</returns>
        public bool IsSuccess()
        {
            return !_dictErrors.Any();
        }

        /// <summary>
        /// Checks if the result contains any error
        /// </summary>
        /// <returns>true if there is a error</returns>
        public bool HasError()
        {
            return _dictErrors.Any();
        }

        /// <summary>
        /// Returns all error strings in that result as one combined string
        /// with a New Line as separator
        /// </summary>
        /// <returns>Error string</returns>
        public string ToErrorString()
        {
            return string.Join(Environment.NewLine, _dictErrors.SelectMany(x => x.Value));
        }

        /// <summary>
        /// Returns all error strings in that result as one combined string
        /// with the specified string as separator
        /// </summary>
        /// <param name="separator">The separator</param>
        /// <returns>Error string</returns>
        public string ToErrorString(string separator)
        {
            return string.Join(separator, _dictErrors.Select(x=>x.Value));
        }

        /// <summary>
        /// Combines to Results and joins their errors if any
        /// </summary>
        /// <param name="result">The result to add</param>
        /// <returns>Itself</returns>
        public Result Add(IResult result)
        {
            if(!result.IsSuccess())
            {
                foreach(KeyValuePair<UserError, IReadOnlyList<string>> valuePair in result)
                {
                    if(!_dictErrors.ContainsKey(valuePair.Key))
                    {
                        _dictErrors.Add(valuePair.Key,new List<string>());
                    }
                    foreach (string valueString in valuePair.Value)
                    {
                        _dictErrors[valuePair.Key].Add(valueString);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds an error to this result
        /// </summary>
        /// <param name="errorMessage">The message to add</param>
        /// <param name="error">The error code to set</param>
        /// <returns>Itself</returns>
        public Result Add(string errorMessage, UserError error)
        {
            return Add(error, errorMessage);
        }

        /// <summary>
        /// Adds an error message to this result under the provided error key
        /// If the key is new a new entry and error message list will be created
        /// </summary>
        /// <param name="error">The error code</param>
        /// <param name="errorMessage">The error message</param>
        /// <returns>Itself</returns>
        private Result Add(UserError error, string errorMessage = "Error message was not set")
        {
            if(_dictErrors.TryGetValue(error,out var stringList))
            {
                stringList.Add(errorMessage);
            }
            else
            {
                _dictErrors.Add(error,new List<string>());
                _dictErrors[error].Add(errorMessage);
            }
            return this;
        }

        /// <summary>
        /// Checks if a char is a digit
        /// </summary>
        /// <param name="c">The char</param>
        /// <returns>true if a digit, otherwise false</returns>
        private bool IsDigit(char c) { return c >= '0' && c <= '9'; }

        /// <summary>
        /// Checks if a char is a Letter or digit
        /// </summary>
        /// <param name="c">The char</param>
        /// <returns>true if a letter or digit, otherwise false</returns>
        private bool IsLetterOrDigit(char c) { return IsUpper(c) || IsLower(c) || IsDigit(c); }

        /// <summary>
        /// Checks if a char is lowercase
        /// </summary>
        /// <param name="c">The char</param>
        /// <returns>true if lowercase otherwise false</returns>
        private bool IsLower(char c) { return c >= 'a' && c <= 'z'; }

        /// <summary>
        /// Checks if a char is uppercase
        /// </summary>
        /// <param name="c">The char</param>
        /// <returns>true if uppercase otherwise false</returns>
        private bool IsUpper(char c) { return c >= 'A' && c <= 'Z'; }

        /// <summary>
        /// Checks if this Result contains an error code
        /// </summary>
        /// <param name="key">The error code</param>
        /// <returns>true if the code is found, otherwise false</returns>
        public bool ContainsKey(UserError key) { return _dictErrors.ContainsKey(key); }

        /// <summary>
        /// Tries to get error messages for an error code in this result
        /// </summary>
        /// <param name="key">the error code</param>
        /// <param name="value">the messages if found</param>
        /// <returns>true if found, otherwise false</returns>
        public bool TryGetValue(UserError key, out IReadOnlyList<string> value)
        {
            if (_dictErrors.TryGetValue(key, out var list))
            {
                value = list;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Error Code Accessor
        /// </summary>
        /// <param name="key">Error Code</param>
        /// <returns>Error Messages Collection</returns>
        public IReadOnlyList<string> this[UserError key] => _dictErrors[key];

        /// <summary>
        /// Key Collection of Error Codes
        /// </summary>
        public IEnumerable<UserError> Keys => _dictErrors.Keys;

        /// <summary>
        /// Values Collection of Error Messages per error code
        /// </summary>
        public IEnumerable<IReadOnlyList<string>> Values => _dictErrors.Values;

        /// <summary>
        /// Value Collection of Error messages in one single collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetValuesFlattened()
        {
            return _dictErrors.Values.SelectMany(x => x);
        } 

        /// <summary>
        /// Enumerator support for error codes/error messages dictionary 
        /// </summary>
        /// <returns>enumerator</returns>
        public IEnumerator<KeyValuePair<UserError, IReadOnlyList<string>>> GetEnumerator()
        {
            foreach(KeyValuePair<UserError, List<string>> valuePair in _dictErrors)
            {
                yield return new KeyValuePair<UserError, IReadOnlyList<string>>(valuePair.Key,valuePair.Value);
            }
        }

        /// <summary>
        /// Returns how many Error Codes are in this result present
        /// </summary>
        public int Count => _dictErrors.Count;

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }

    /// <summary>
    /// Holds Results of function calls and provides detailed error information
    /// </summary>
    public interface IResult : IReadOnlyDictionary<UserError, IReadOnlyList<string>>
    {
        /// <summary>
        /// Allows to verify if this Result has no error and is a success
        /// </summary>
        /// <returns>true if no error present</returns>
        bool IsSuccess();

        /// <summary>
        /// Allows to verify if this Result has a error and is a failure
        /// </summary>
        /// <returns>true if there is a error present</returns>
        bool HasError();
        /// <summary>
        /// Returns all error messages a one error string with each error message on a new line
        /// </summary>
        /// <returns>Error String</returns>
        string ToErrorString();
        /// <summary>
        /// Provides all error messages as a Collection
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetValuesFlattened();
    }

    /// <summary>
    /// Result holding a Return Type in case of success
    /// </summary>
    /// <typeparam name="T">The Return Type</typeparam>
    public interface IResult<out T> : IResult
    {
        /// <summary>
        /// Gets the Return Value
        /// </summary>
        T ReturnValue { get; }
    }
}