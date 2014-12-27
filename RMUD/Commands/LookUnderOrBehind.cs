﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD.Commands
{
    internal class LookUnderOrBehind : CommandFactory, DeclaresRules
    {
        public override void Create(CommandParser Parser)
        {
            Parser.AddCommand(
                Sequence(
                    Or(
                        KeyWord("LOOK"),
                        KeyWord("L")),
                    RelativeLocation("RELLOC"),
                    Object("OBJECT", InScope)),
                "Look on, in, under or behind an object.")
                .Manual("Lists object that are in, on, under, or behind the object specified.")
                .Check("can look relloc?", "OBJECT", "ACTOR", "OBJECT", "RELLOC")
                .Perform("look relloc", "OBJECT", "ACTOR", "OBJECT", "RELLOC");
        }

        public void InitializeGlobalRules()
        {
            GlobalRules.DeclareCheckRuleBook<MudObject, MudObject, RelativeLocations>("can look relloc?", "[Actor, Item, Relative Location] : Can the actor look in/on/under/behind the item?");

            GlobalRules.Check<MudObject, MudObject, RelativeLocations>("can look relloc?")
                .Do((actor, item, relloc) => GlobalRules.IsVisibleTo(actor, item))
                .Name("Container must be visible rule.");

            GlobalRules.Check<MudObject, MudObject, RelativeLocations>("can look relloc?")
                .When((actor, item, relloc) => !(item is Container) || (((item as Container).LocationsSupported & relloc) != relloc))
                .Do((actor, item, relloc) =>
                {
                    Mud.SendMessage(actor, "You can't look " + Mud.GetRelativeLocationName(relloc) + " that.");
                    return CheckResult.Disallow;
                })
                .Name("Container must support relloc rule.");

            GlobalRules.Check<MudObject, MudObject, RelativeLocations>("can look relloc?")
                .When((actor, item, relloc) => (relloc == RelativeLocations.In) && !GlobalRules.ConsiderValueRule<bool>("open?", item, item))
                .Do((actor, item, relloc) =>
                {
                        Mud.SendMessage(actor, "^<the0> is closed.", item);
                        return CheckResult.Disallow;
                })
                .Name("Container must be open to look in rule.");

            GlobalRules.Check<MudObject, MudObject, RelativeLocations>("can look relloc?")
                .Do((actor, item, relloc) => CheckResult.Allow)
                .Name("Default allow looking relloc rule.");

            GlobalRules.DeclarePerformRuleBook<MudObject, MudObject, RelativeLocations>("look relloc", "[Actor, Item, Relative Location] : Handle the actor looking on/under/in/behind the item.");

            GlobalRules.Perform<MudObject, MudObject, RelativeLocations>("look relloc")
                .Do((actor, item, relloc) =>
                {
                    var contents = (item as Container).GetContents(relloc);
                    if (contents.Count > 0)
                    {
                        Mud.SendMessage(actor, "^" + Mud.GetRelativeLocationName(relloc) + " <the0> is ", item);
                        foreach (var thing in contents)
                            Mud.SendMessage(actor, "  <a0>", thing);
                    }
                    else
                        Mud.SendMessage(actor, "There is nothing " + Mud.GetRelativeLocationName(relloc) + " <the0>.", item);
                    return PerformResult.Continue;
                })
                .Name("List contents in relative location rule.");
        }
    }
}