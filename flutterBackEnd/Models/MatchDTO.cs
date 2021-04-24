using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace flutterBackEnd.Models
{
    public class MatchDTO
    {
        public int id { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public int level { get; set; }
        public String Captain { get; set; }
        //      public List<String> players { get; set; }
        public List<String> players { get; set; }
}
}