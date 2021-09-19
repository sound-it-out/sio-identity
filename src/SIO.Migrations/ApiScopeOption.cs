using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIO.Migrations
{
    public class ApiScopeOption
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
    }
}
