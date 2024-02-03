using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UChat
{
    public class Message
    {
        public string Sender {  get; set; }
        public string Content { get; set; }

        public StorageFile Audio { get; set; }
    }
}
