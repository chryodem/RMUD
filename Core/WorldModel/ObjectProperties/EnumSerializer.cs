﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMUD
{
    public class EnumSerializer<EnumType> : TypeSerializer
    {
        public override string ConvertToString(object @object)
        {
            if (@object == null) return "%NULL";
            else return @object.ToString();
        }

        public override object ConvertFromString(string str)
        {
            return Enum.Parse(typeof(EnumType), str, true);            
        }
    }
}
