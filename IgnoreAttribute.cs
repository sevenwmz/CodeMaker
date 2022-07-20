using System;
using System.Collections.Generic;
using System.Text;

namespace CodeMaker
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple =false)]
    public class IgnoreAttribute : Attribute
    {
    }
}
