using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace UChat.Services.Interfaces
{
    public interface IApiService
    {
        Task<string> SendRequestAsync(IBuffer buffer, String name);
    }
}
