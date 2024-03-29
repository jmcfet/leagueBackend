﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace flutterBackEnd.Models
{
    public class Match
    {
        public int id { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public int skillLevel { get; set; }
        public int year { get; set; }
        public String captain { get; set; }
        public virtual List<ApplicationUser> players { get; set; }
    }
}