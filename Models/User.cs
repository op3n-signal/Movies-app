using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace L08HandsOn.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int MovieId { get; set; }
        public int TimesWatched { get; set; }
    }
}
