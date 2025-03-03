using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stack
{
    public class GlobalConnection
    {
        public static HubConnection connection;
        public static String ruta = "https://localhost:7121/stackhub";
    }
}
