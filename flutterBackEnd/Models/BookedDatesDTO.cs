using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace flutterBackEnd.Models
{
    public class BookedDatesDTO
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int month { get; set; }
        public int level { get; set; }
        public bool isCaptain { get; set; }
        public int numTimesCaptain { get; set; }
       
        public string status { get; set; }
        public BookedDatesDTO() { }
        public BookedDatesDTO(int id,String Name,int month,int level,bool isCaptain,int numTimesCaptain,String status)
        {
            this.id = id;
            this.Name = Name;
            this.isCaptain = isCaptain;
            this.level = level;
            this.numTimesCaptain = numTimesCaptain;
            this.status = status;
            this.month = month;
        }
    }
}


