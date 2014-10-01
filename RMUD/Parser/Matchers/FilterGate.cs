﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD
{
    internal class FilterGate : ICommandTokenMatcher
    {
        public Func<PossibleMatch, MatchContext, bool> Filter;
		public String FilterHelpText;

		internal FilterGate(Func<PossibleMatch, MatchContext, bool> Filter, String FilterHelpText)
		{
			this.Filter = Filter;
			this.FilterHelpText = FilterHelpText;
		}

        public List<PossibleMatch> Match(PossibleMatch State, MatchContext Context)
        {
            var R = new List<PossibleMatch>();
			if (Filter(State, Context))
				R.Add(State);
			return R;
        }

		public String Emit() { return "<" + FilterHelpText + ">"; }
    }
}
