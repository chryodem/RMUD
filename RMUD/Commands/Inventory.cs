﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD.Commands
{
	internal class Inventory : CommandFactory
	{
		public override void Create(CommandParser Parser)
		{
			Parser.AddCommand(
				new Or(
					new KeyWord("i", false),
					new KeyWord("inv", false),
					new KeyWord("inventory", false)),
				new InventoryProcessor(),
				"See what you are carrying.");
		}
	}

	internal class InventoryProcessor : ICommandProcessor
	{
		public void Perform(PossibleMatch Match, Actor Actor)
		{
			if (Actor.ConnectedClient == null) return;

			var builder = new StringBuilder();

			if (Actor.Held.Count == 0) builder.Append("You have noMudObject.\r\n");
			else
			{
				builder.Append("You are carrying..\r\n");
				foreach (var item in Actor.Held)
				{
					builder.Append("  ");
					builder.Append(item.Indefinite);
					builder.Append("\r\n");
				}
			}

			Mud.SendMessage(Actor, MessageScope.Single, builder.ToString());
		}
	}
}
