﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD
{
    public class RuleBook
    {
        public String Name;
        public String Description;
        public List<Type> ArgumentTypes = new List<Type>();
        public Type ResultType;
        internal List<Rule> Rules = new List<Rule>();

        public bool CheckArgumentTypes(Type ResultType, params Type[] ArgTypes)
        {
            if (this.ResultType != ResultType) return false;
            if (ArgTypes.Length != ArgumentTypes.Count) return false;
            for (int i = 0; i < ArgTypes.Length; ++i)
                if (!ArgumentTypes[i].IsAssignableFrom(ArgTypes[i]))
                    return false;
            return true;
        }

        public virtual void AddRule(Rule Rule) { throw new NotImplementedException(); }
        public virtual void DeleteRule(String ID) { throw new NotImplementedException(); }
    }

    public class CheckRuleBook : RuleBook
    {
        public CheckRuleBook()
        {
            ResultType = typeof(CheckRuleResult);
        }

        public CheckRuleResult Consider(params Object[] Args)
        {
            foreach (var _rule in Rules)
            {
                var rule = _rule as Rule<CheckRuleResult>;
                if (rule.WhenClause == null || rule.WhenClause.Invoke(Args))
                {
                    if (GlobalRules.LogTo != null)
                    {
                        GlobalRules.LogTo.Send(Name + "<" + String.Join(", ", ArgumentTypes.Select(t => t.Name)) + "> -> " + ResultType.Name + " : " + (String.IsNullOrEmpty(rule.DescriptiveName) ? "NONAME" : rule.DescriptiveName) + "\r\n");
                    }

                    var r = rule.BodyClause == null ? CheckRuleResult.Continue : rule.BodyClause.Invoke(Args);
                    if (r != CheckRuleResult.Continue) return r;
                }
            }
            return CheckRuleResult.Continue;
        }

        public override void AddRule(Rule Rule)
        {
            if (!(Rule is Rule<CheckRuleResult>)) throw new InvalidOperationException();
            Rules.Insert(0, Rule);
        }

        public override void DeleteRule(string ID)
        {
            Rules.RemoveAll(r => r.ID == ID);
        }
    }


    public class ActionRuleBook : RuleBook
    {
        public ActionRuleBook()
        {
            ResultType = typeof(RuleResult);
        }

        public RuleResult Consider(params Object[] Args)
        {
            foreach (var _rule in Rules)
            {
                var rule = _rule as Rule<RuleResult>;
                if (rule.WhenClause == null || rule.WhenClause.Invoke(Args))
                {
                    if (GlobalRules.LogTo != null)
                    {
                        GlobalRules.LogTo.Send(Name + "<" + String.Join(", ", ArgumentTypes.Select(t => t.Name)) + "> -> " + ResultType.Name + " : " + (String.IsNullOrEmpty(rule.DescriptiveName) ? "NONAME" : rule.DescriptiveName) + "\r\n");
                    }

                    var r = rule.BodyClause == null ? RuleResult.Default : rule.BodyClause.Invoke(Args);
                    if (r != RuleResult.Continue) return r;
                }
            }
            return RuleResult.Default;
        }

        public override void AddRule(Rule Rule)
        {
            if (!(Rule is Rule<RuleResult>)) throw new InvalidOperationException();
            Rules.Insert(0, Rule as Rule<RuleResult>);
        }

        public override void DeleteRule(string ID)
        {
            Rules.RemoveAll(r => r.ID == ID);
        }
    }

    public class ValueRuleBook<RT> : RuleBook
    {
        public ValueRuleBook()
        {
            ResultType = typeof(RT);
        }

        public RT Consider(out bool ValueReturned, params Object[] Args)
        {
            ValueReturned = false;
            foreach (var rule in Rules)
                if (rule.WhenClause == null || rule.WhenClause.Invoke(Args))
                {
                    if (GlobalRules.LogTo != null)
                    {
                        GlobalRules.LogTo.Send(Name + "<" + String.Join(", ", ArgumentTypes.Select(t => t.Name)) + "> -> " + ResultType.Name + " : " + (String.IsNullOrEmpty(rule.DescriptiveName) ? "NONAME" : rule.DescriptiveName) + "\r\n");
                    }

                    ValueReturned = true;
                    return (rule as Rule<RT>).BodyClause.Invoke(Args);
                }
            return default(RT);
        }

        public override void AddRule(Rule Rule)
        {
            if (!(Rule is Rule<RT>)) throw new InvalidOperationException();
            Rules.Insert(0, Rule as Rule<RT>);
        }

        public override void DeleteRule(string ID)
        {
            Rules.RemoveAll(r => r.ID == ID);
        }
    }
}
