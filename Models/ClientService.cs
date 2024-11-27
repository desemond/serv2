using Server.Models;

namespace Server.Services
{
    public class ClientService
    {
        private readonly List<ClientLevel> _clients = new();

        public List<ClientLevel> GetAllClients() => _clients;

        public ClientLevel GetClient(string clientName) =>
            _clients.FirstOrDefault(c => c.ClientName == clientName);

        public void AddOrUpdateClient(ClientLevel newClient) =>
            ClientLevel.AddOrUpdateClientLevel(_clients, newClient);
    }
}
