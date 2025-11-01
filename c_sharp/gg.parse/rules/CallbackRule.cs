// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

namespace gg.parse.rules
{
    public delegate void RuleCallbackAction<T>(IRule rule, T[] data, ParseResult? result = null) where T : IComparable<T>;

    /// <summary>
    /// Will give a callback to the client code when its associated rule meets a certain 
    /// result. Convenient when debugging large rule graphs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CallbackRule<T> : MetaRuleBase<T> where T : IComparable<T>
    {
        public enum CallbackCondition
        {
            Success,
            Failure,
            Any
        }

        public CallbackCondition Condition { get; init; }

        public RuleCallbackAction<T>? ParseStartCallback { get; init; }

        public RuleCallbackAction<T>? ResultCallback { get; init; }

        public CallbackRule(
            string name, 
            IRule subject,            
            RuleCallbackAction<T> callback, 
            CallbackCondition condition = CallbackCondition.Success
        )
        : base(name, AnnotationPruning.Root, 0, subject)
        {
            ResultCallback = callback;
            Condition = condition;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            Assertions.RequiresNotNull(Subject);

            ParseStartCallback?.Invoke(this, input);
           
            var result = Subject.Parse(input, start);

            if (ResultCallback != null)
            {
                switch (Condition)
                {
                    case CallbackCondition.Success:
                        if (result)
                        {
                            ResultCallback(this, input, result);
                        }
                        break;
                    case CallbackCondition.Failure:
                        if (!result)
                        {
                            ResultCallback(this, input, result);
                        }
                        break;
                    case CallbackCondition.Any:
                        ResultCallback(this, input, result);
                        break;
                }
            }

            return result;
        }

        public override CallbackRule<T> CloneWithSubject(IRule subject) =>
            new (Name, subject, ResultCallback!, Condition);       
    }
}