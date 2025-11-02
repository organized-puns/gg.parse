Rules
=====

The rules making up the tokenizer and parser come in three types:

* Data rules, rules which parse the input. Eg, parsing a literal.
* Meta rules, rules which use one other rule to determine the outcome of their Parse operation. Eg parse one or more literals. 
* Rule compositions, rules which use zero or more other rules, eg parse a sequence of a literal, followed by another literal.

These types are implemented as:

```mermaid
classDiagram
    namespace interfaces {
        class IRule {
            <<interface>>
            string Name
            int Precedence
            AnnotationPruning Prune
            ParseResult Parse(Array input, int start)
        }

        class IMetaRule {
            <<interface>>
            IRule? Subject
            IMetaRule CloneWithSubject(IRule subject)
            void MutateSubject(IRule subject)
        }

        class IRuleComposition {
            <<interface>>
            IEnumerable<IRule>? Rules
            int Count
            IRule? this[int index]
            IRuleComposition CloneWithComposition(IEnumerable<IRule> composition)
            void MutateComposition(IEnumerable<IRule> composition)
        }
    }

    namespace implementations {
        class RuleBase~T~ {
        }

        class RuleCompositionBase~T~ {
        }

        class MetaRuleBase~T~ {
        }
    }

    IRule <|-- IMetaRule
    IRule <|-- IRuleComposition
    IRule <|.. RuleBase~T~
    
    IMetaRule <|.. MetaRuleBase~T~
    RuleBase~T~ <|-- MetaRuleBase~T~
    
    IRuleComposition <|.. RuleCompositionBase~T~
    RuleBase~T~ <|-- RuleCompositionBase~T~    
```

### Categories by use-case

The rules implementations come in the following categories:

#### Data rules

Parse the data either characters or token.

* [MatchAnyData](./match-any-data.md)
* [MatchDataRange](./match-data-range.md)
* [MatchDataSequence](./match-data-sequence.md)
* MatchDataSet
* MatchSingleData

#### Meta rules (IMetaRule)

Return a result based on the outcome of its subject.

* [MatchCount](./match-count.md) 

#### Look ahead rules (IMetaRule)

Return a result based on the outcome of its subject, but do not change the data pointer.

* [MatchCondition](./match-condition.md)
* [MatchNot](./match-not.md)
* [SkipRule](./skip_rule.md)

#### Sequences (IRuleComposition)

Return the result based on whether all subrules succeed in a specific order

* MatchSequence


#### Options (IRuleComposition)

Return the results based on if one of the subrules succeed

* MatchOneOf
* [MatchEvaluation](./match-evaluation.md)

#### Misc

* Callback
* Log
* [RuleReference](./rule-reference.md)


