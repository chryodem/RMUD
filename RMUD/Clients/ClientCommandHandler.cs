﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD
{
	public interface ClientCommandHandler
	{
		void HandleCommand(Actor Actor, String Command);
	}
}
