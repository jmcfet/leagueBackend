using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace flutterBackEnd.Models
{
    public class BookedDates
    {
        public int id { get; set; }
     //   public string Name { get; set; }
        public int month { get; set; }
        //    public int level { get; set; }
        //    public bool isCaptain { get; set; }
        //     public int numTimesCaptain { get; set; }
       public virtual ApplicationUser user { get; set; }
        public string status { get; set; }
    }
}


