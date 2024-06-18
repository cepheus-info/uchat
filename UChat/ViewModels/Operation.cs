using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UChat.ViewModels
{
    public struct Operation
    {
        public Operation(DateTime timeStamp, string description)
        {
            TimeStamp = timeStamp;
            Description = description;
        }

        public DateTime TimeStamp { get; set; }

        public string Description { get; set; }
    }
}
