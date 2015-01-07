﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace RMUD
{
    public static partial class Core
    {
        internal static Mutex DatabaseLock = new Mutex();
        public static bool ShuttingDown { get; private set; }
        public static Settings SettingsObject;
        public static WorldDataService Database;
        public static Action OnShutDown = null;

        public static void TiePlayerToClient(Client Client, Actor Actor)
        {
            Client.Player = Actor;
            Actor.ConnectedClient = Client;
        }

        public static void AddPlayer(Actor Actor)
        {
            Actor.Rank = 500;
            MudObject.Move(Actor, MudObject.GetObject(Core.SettingsObject.NewPlayerStartRoom));
            Core.EnqueuActorCommand(Actor, "look");
            GlobalRules.ConsiderPerformRule("player joined", Actor);
        }

        public static void RemovePlayer(Actor Actor)
        {
            GlobalRules.ConsiderPerformRule("player left", Actor);
            Actor.ConnectedClient = null;
            MudObject.Move(Actor, null);
        }

        public static bool Start(WorldDataService Database, params System.Reflection.Assembly[] Assemblies)
        {
            ShuttingDown = false;

            try
            {
                InitializeCommandProcessor();

                var assemblies = new List<Assembly>(Assemblies);
                assemblies.Insert(0, Assembly.GetExecutingAssembly());

                GlobalRules.DeclarePerformRuleBook("at startup", "[] : Considered when the engine is started.");

                foreach (var assembly in assemblies.Distinct())
                    foreach (var type in assembly.GetTypes())
                        foreach (var method in type.GetMethods())
                            if (method.IsStatic && method.Name == "AtStartup")
                                method.Invoke(null, null);

                PersistentValueSerializer.AddGlobalSerializer(new BitArraySerializer());

                Core.Database = Database;
                Database.Initialize();

                GlobalRules.ConsiderPerformRule("at startup");
                StartCommandProcesor();
            }
            catch (Exception e)
            {
                LogError("Failed to start mud engine.");
                LogError(e.Message);
                LogError(e.StackTrace);
                throw;
            }
            return true;
        }

        public static void Shutdown()
        {
            ShuttingDown = true;
        }
    }
}
