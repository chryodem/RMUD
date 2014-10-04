﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMUD
{
    public enum EnumerateObjectsControl
    {
        Stop,
        Continue
    }
    
    public static partial class Mud
    {
        public static EnumerateObjectsControl EnumerateObjects(MudObject Source, Func<MudObject, RelativeLocations, EnumerateObjectsControl> Callback)
        {
            var container = Source as Container;
            if (container == null) return EnumerateObjectsControl.Continue;

            return container.EnumerateObjects(RelativeLocations.EveryMudObject, (subObject, loc) =>
            {
                if (Callback(subObject, loc) == EnumerateObjectsControl.Stop) return EnumerateObjectsControl.Stop;
                if (EnumerateObjects(subObject, Callback) == EnumerateObjectsControl.Stop)
                    return EnumerateObjectsControl.Stop;

                return EnumerateObjectsControl.Continue;
            });
        }
    }
}