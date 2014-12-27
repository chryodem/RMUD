﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD.Commands
{
    internal class Unlock : CommandFactory, DeclaresRules
    {
        public override void Create(CommandParser Parser)
        {
            Parser.AddCommand(
                Sequence(
                    KeyWord("UNLOCK"),
                    BestScore("ITEM",
                        MustMatch("I couldn't figure out what you're trying to unlock.",
                            Object("ITEM", InScope))),
                    OptionalKeyWord("WITH"),
                    BestScore("KEY",
                        MustMatch("I couldn't figure out what you're trying to unlock that with.",
                            Object("KEY", InScope, PreferHeld)))),
                "Unlock something with something.")
                .Manual("Use the KEY to unlock the ITEM.")
                .Check("can lock?", "ITEM", "ACTOR", "ITEM", "KEY")
                .Perform("unlocked", "ITEM", "ACTOR", "ITEM", "KEY");
        }

        public void InitializeGlobalRules()
        {
            GlobalRules.DeclarePerformRuleBook<MudObject, MudObject, MudObject>("unlocked", "[Actor, Item, Key] : Handle the actor unlocking the item with the key.");

            GlobalRules.Perform<MudObject, MudObject, MudObject>("unlocked").Do((actor, target, key) =>
            {
                Mud.SendMessage(actor, "You unlock <the0>.", target);
                Mud.SendExternalMessage(actor, "<a0> unlocks <a1> with <a2>.", actor, target, key);
                return PerformResult.Continue;
            });
        }
    }
}
