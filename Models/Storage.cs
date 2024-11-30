using Server.Models;

namespace WebSocketServer.Models
{
    public class Storage
    {
        public static List<ClientLevel> clients  = new List<ClientLevel>();
        public static string path ="";
        public static int time;
    }
}
