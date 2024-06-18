using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UChat.Models
{
    public class ExceptionMessage
    {
        public ExceptionMessage(DateTime timeStamp, string message)
        {
            TimeStamp = timeStamp;
            Message = message;
        }

        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }
    }
}
