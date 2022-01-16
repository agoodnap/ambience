using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambience.Domain
{
    public class Ambience
    {
        public string Name { get; set; }
        public string[]? Links { get; set; }

        public Ambience(string InName, string[]? InLinks)
        {
            Name = InName;
            Links = InLinks;
        }
    }
}
