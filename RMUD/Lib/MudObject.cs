﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD
{
    public enum ObjectState
    {
        Unitialized,
        Alive,
        Destroyed,
    }

    public class MudObjectRules : DeclaresRules
    {
        public void InitializeRules()
        {
            GlobalRules.DeclareValueRuleBook<MudObject, MudObject, String, String>("printed name", "[Viewer, Object, Article -> String] : Find the name that should be displayed for an object.");

            GlobalRules.Value<MudObject, MudObject, String, String>("printed name")
               .Last
               .Do((viewer, thing, article) => article + " " + thing.Short)
               .Name("Default name of a thing.");
       }
    }

	public partial class MudObject : HasRules
    {
        public ObjectState State = ObjectState.Unitialized; 
		public String Path { get; internal set; }
		public String Instance { get; internal set; }

        public bool IsNamedObject { get { return Path != null; } }
        public bool IsAnonymousObject { get { return Path == null; } }
        public bool IsInstance { get { return IsNamedObject && Instance != null; } }
        public String GetFullName() { return Path + "@" + Instance; }

        public bool IsPersistent { get; set; }
        
		public virtual void Initialize() { }
        public virtual void HandleMarkedUpdate() { }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Path)) return this.GetType().Name;
            else return Path;
        }

        public String Short;
        public String Long;
        public String Article = "a";
		public NounList Nouns { get; set; }
        public MudObject Location { get; set; }
        public RuleSet Rules { get; set; }
        
        public String Indefinite(MudObject RequestedBy) 
        {
            return GlobalRules.ConsiderValueRule<String>("printed name", RequestedBy, this, Article);
        }

        public String Definite(MudObject RequestedBy)
        {
            return GlobalRules.ConsiderValueRule<String>("printed name", RequestedBy, this, "the");
        }

		public MudObject()
		{
			Nouns = new NounList();
            State = ObjectState.Alive;
            IsPersistent = false;
            Rules = null;
		}

        public MudObject(String Short, String Long)
        {
            Nouns = new NounList();
            this.Short = Short;
            this.Long = Long;
            Nouns.Add(Short.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            var firstChar = Short.ToLower()[0];
            if (firstChar == 'a' || firstChar == 'e' || firstChar == 'i' || firstChar == 'o' || firstChar == 'u')
                Article = "an";

            State = ObjectState.Alive;
            IsPersistent = false;

            Rules = null;
        }

        public void SimpleName(String Short, params String[] Synonyms)
        {
            this.Short = Short;
            Nouns.Add(Short.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            Nouns.Add(Synonyms);
        }

        public void Destroy(bool DestroyChildren)
        {
            State = ObjectState.Destroyed;
            MudObject.ForgetInstance(this);

            if (DestroyChildren && this is Container)
                foreach (var child in (this as Container).EnumerateObjects())
                    if (child.State != ObjectState.Destroyed)
                        child.Destroy(true);
        }
    }
}
