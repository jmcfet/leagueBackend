using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace flutterBackEnd.Models
{
    public class BookedDatesDTO
    {
        public int id { get; set; }
        public Controllers.userdto user { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public bool isCaptain { get; set; }
           
        public string status { get; set; }
        public BookedDatesDTO() { }
        public BookedDatesDTO(int id,int month,int year,bool isCaptain,String status)
        {
            this.id = id;
            this.isCaptain = isCaptain;
            this.status = status;
            this.year = year;
            this.month = month;
        }
    }
}


