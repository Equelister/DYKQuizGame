using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Net
{
    class Hub
    {

        public Guid UID { get; set; }
        public string Name { get; set; }
        public List<Client> Users { get; set; }
        public int MaxSize { get; set; }

        public Hub(string name)
        {
            UID = Guid.NewGuid();
            Name = name;
            Users = new List<Client>();
            MaxSize = 8;
        }

        public Hub(string name, int maxSize)
        {
            UID = Guid.NewGuid();
            Name = name;
            Users = new List<Client>();
            MaxSize = maxSize;
        }

        public bool AddClient(Client client)
        {
            if(Users.Count <= MaxSize)
            {
                Users.Add(client);
                return true;
            }
            else
            {
                Console.WriteLine($"Client [{client.UID}] tried to connect into hub [{this.UID}] but it was full.");
                return false;
            }
        }

        
        
    }
}
