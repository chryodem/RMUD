﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD
{
    public class BasicDoor : MudObject, OpenableRules, TakeRules
	{
		public BasicDoor()
		{
			this.Nouns.Add("DOOR", "CLOSED");
            Open = false;
		}

		#region IOpenable

		public bool Open { get; set; }

		CheckRule OpenableRules.CheckOpen(Actor Actor)
		{
            if (Open) return CheckRule.Disallow("It's already open.");
            else return CheckRule.Allow();
		}

		CheckRule OpenableRules.CheckClose(Actor Actor)
		{
            if (Open) return CheckRule.Allow();
            else return CheckRule.Disallow("It's already closed.");
		}

		RuleHandlerFollowUp OpenableRules.HandleOpen(Actor Actor)
		{
			Open = true;
            Nouns.RemoveAll(n => n == "CLOSED");
            Nouns.Add("OPEN");
            return RuleHandlerFollowUp.Continue;
		}

		RuleHandlerFollowUp OpenableRules.HandleClose(Actor Actor)
		{
			Open = false;
            Nouns.RemoveAll(n => n == "OPEN");
            Nouns.Add("CLOSED");
            return RuleHandlerFollowUp.Continue;
		}

		#endregion

		CheckRule TakeRules.Check(Actor Actor)
		{
			return CheckRule.Disallow("Doors only make good doors if you leave them where they are at.");
		}

        RuleHandlerFollowUp TakeRules.Handle(Actor Actor) { return RuleHandlerFollowUp.Continue; }
	}
}
