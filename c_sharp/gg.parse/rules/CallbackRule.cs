// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.rules
{
    public delegate void RuleCallbackAction<T>(IRule rule, T[] data, ParseResult? result = null) where T : IComparable<T>;

    /// <summary>
    /// Will give a callback to the client code when its associated rule meets a certain 
    /// result. Convenient when debugging large rule graphs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CallbackRule<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public enum CallbackCondition
        {
            Success,
            Failure,
            Any
        }

        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];
        
        public int Count => 1;

        public RuleBase<T>? this[int index] => Rule;

        public CallbackCondition Condition { get; init; }

        public RuleCallbackAction<T>? ParseStartCallback { get; init; }

        public RuleCallbackAction<T>? ResultCallback { get; init; }

        public CallbackRule(

            string name, 
            RuleBase<T> rule,            
            RuleCallbackAction<T> callback, 
            CallbackCondition condition = CallbackCondition.Success
        )
            : base(name)
        {
            Rule = rule;
            ResultCallback = callback;
            Condition = condition;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            ParseStartCallback?.Invoke(this, input);
           
            var result = Rule.Parse(input, start);

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

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition) =>
            new CallbackRule<T>(
                Name, 
                composition.First(), 
                ResultCallback!, 
                Condition
            );
        
    }
}