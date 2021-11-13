﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Net
{
    class Hub
    {

        public Guid GUID { get; set; }
        public string Name { get; set; }
        public List<Client> Users { get; set; }
        public int MaxSize { get; set; }
        public int JoinCode { get; set; }

        public Hub(string name)
        {
            GUID = Guid.NewGuid();
            Name = name;
            Users = new List<Client>();
            MaxSize = 8;
            JoinCode = GenerateJoinCode();
        }

        public Hub(string name, int maxSize)
        {
            GUID = Guid.NewGuid();
            Name = name;
            Users = new List<Client>();
            MaxSize = maxSize;
            JoinCode = GenerateJoinCode();
        }

        public int GenerateJoinCode()
        {
            return new Random().Next(9000)+1000;
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
                Console.WriteLine($"Client [{client.UID}] tried to connect into hub [{this.GUID}] but it was full.");
                return false;
            }
        }

        
        
    }
}
