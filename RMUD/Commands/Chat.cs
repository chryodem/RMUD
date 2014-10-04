﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD.Commands
{
	internal class Chat : CommandFactory
	{
        public override void Create(CommandParser Parser)
        {
            Parser.AddCommand(
                new Sequence(
                    new KeyWord("SUBSCRIBE", false),
                    new FailIfNoMatches(
                        new ChatChannelNameMatcher("CHANNEL"),
                        "I don't recognize that channel.\r\n")),
                new SubscribeProcessor(),
                "Subscribe to a chat channel.");

            Parser.AddCommand(
                new Sequence(
                    new KeyWord("UNSUBSCRIBE", false),
                    new FailIfNoMatches(
                        new ChatChannelNameMatcher("CHANNEL"),
                        "I don't recognize that channel.\r\n")),
                new UnsubscribeProcessor(),
                "Unubscribe from a chat channel.");

            Parser.AddCommand(
                new KeyWord("CHANNELS", false),
                new ListChannelsProcessor(),
                "List all available chat channels.");

            Parser.AddCommand(
                new Sequence(
                    new ChatChannelNameMatcher("CHANNEL"),
                    //new FailIfNoMatches(
                        new Rest("TEXT")),
                    //    "You have to actually say someMudObject to use the chat channel.\r\n")),
                new ChatProcessor(),
                "Chat on a channel.");

            Parser.AddCommand(
               new Sequence(
                   new KeyWord("RECALL", false),
                   new FailIfNoMatches(
                       new ChatChannelNameMatcher("CHANNEL"),
                       "I don't recognize that channel.\r\n"),
                   new Optional(
                       new Number("COUNT"))),
                new RecallProcessor(),
                "Recall past conversation on a channel.");
        }
	}

	internal class SubscribeProcessor : CommandProcessor
	{
        public void Perform(PossibleMatch Match, Actor Actor)
        {
            if (Actor.ConnectedClient == null) return;

            var channel = Match.Arguments.ValueOrDefault("CHANNEL") as ChatChannel;
            if (channel.AccessFilter != null && !channel.AccessFilter(Actor.ConnectedClient))
            {
                Mud.SendMessage(Actor, "You do not have access to that channel.\r\n");
                return;
            }

            if (!channel.Subscribers.Contains(Actor.ConnectedClient))
                channel.Subscribers.Add(Actor.ConnectedClient);
            Mud.SendMessage(Actor, "You are now subscribed to " + channel.Name + ".\r\n");
        }
    }

    internal class UnsubscribeProcessor : CommandProcessor
    {
        public void Perform(PossibleMatch Match, Actor Actor)
        {
            if (Actor.ConnectedClient == null) return;

            var channel = Match.Arguments.ValueOrDefault("CHANNEL") as ChatChannel;
            channel.Subscribers.RemoveAll(c => Object.ReferenceEquals(c, Actor.ConnectedClient));
            Mud.SendMessage(Actor, "You are now unsubscribed from " + channel.Name + ".\r\n");
        }
    }

    internal class ListChannelsProcessor : CommandProcessor
    {
        public void Perform(PossibleMatch Match, Actor Actor)
        {
            if (Actor.ConnectedClient == null) return;

            var builder = new StringBuilder();
            builder.Append("~~~ CHAT CHANNELS ~~~\r\n");
            foreach (var channel in Mud.ChatChannels)
            {
                if (channel.Subscribers.Contains(Actor.ConnectedClient))
                    builder.Append("*");
                builder.Append(String.Format("{0}\r\n", channel.Name));
            }
            builder.Append("\r\n");

            Mud.SendMessage(Actor, builder.ToString());
        }
    }

    internal class ChatProcessor : CommandProcessor
    {
        public void Perform(PossibleMatch Match, Actor Actor)
        {
            if (Actor.ConnectedClient == null) return;

            var channel = Match.Arguments.ValueOrDefault("CHANNEL") as ChatChannel;

            if (!channel.Subscribers.Contains(Actor.ConnectedClient))
            {
                if (channel.AccessFilter != null && !channel.AccessFilter(Actor.ConnectedClient))
                {
                    Mud.SendMessage(Actor, "You do not have access to that channel.\r\n");
                    return;
                }

                channel.Subscribers.Add(Actor.ConnectedClient);
                Mud.SendMessage(Actor, "You are now subscribed to " + channel.Name + ".\r\n");
            }

            var messageBuilder = new StringBuilder();
            messageBuilder.Append(String.Format("[{0}] {1}: \"", channel.Name, Actor.Short));
            Mud.AssembleText(Match.Arguments["TEXT"] as LinkedListNode<String>, messageBuilder);
            messageBuilder.Append("\"\r\n");

            Mud.SendChatMessage(channel, messageBuilder.ToString());
        }
    }

    internal class RecallProcessor : CommandProcessor
    {
        public void Perform(PossibleMatch Match, Actor Actor)
        {
            if (Actor.ConnectedClient == null) return;

            var channel = Match.Arguments.ValueOrDefault("CHANNEL") as ChatChannel;
            if (channel.AccessFilter != null && !channel.AccessFilter(Actor.ConnectedClient))
            {
                Mud.SendMessage(Actor, "You do not have access to that channel.\r\n");
                return;
            }

            int count = 20;
            if (Match.Arguments.ContainsKey("COUNT")) count = (Match.Arguments["COUNT"] as int?).Value;

            var start = channel.ChatHistory.Count - count;
            if (start < 0) start = 0;

            for (int i = start; i < channel.ChatHistory.Count; ++i)
                Mud.SendMessage(Actor, String.Format("{0} : {1}", channel.ChatHistory[i].Time, channel.ChatHistory[i].Message));

        }
    }
}
