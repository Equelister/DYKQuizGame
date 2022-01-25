using DYKServer.Net;
using DYKShared.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Tests
{
    class UserUnitTests
    {
        public Client client;

        [SetUp]
        public void Setup()
        {
            client = new Client();
            client.GUID = Guid.NewGuid();
            client.UserModel = new DYKShared.Model.UserModel("testUserModel");
            client.UserModel.ID = 1;
            Program._users.Add(client);
        }

        [Test]
        public void IsUserAlreadyLoggedInShouldReturnTrue()
        {
            Client testClient = client;
            Program._users.Add(testClient);
            string message = "credsLegit";
            Program.CheckUniqueUser(ref message, testClient);
            Assert.AreEqual("credsNotLegit", message);
        }           

        [Test]
        public void IsUserAlreadyLoggedInShouldReturnFalse()
        {
            string message = "credsLegit";
            Client testClient = new Client();
            testClient.GUID = Guid.NewGuid();
            testClient.UserModel = new DYKShared.Model.UserModel("testUserModel2");
            testClient.UserModel.ID = 2;

            Program.CheckUniqueUser(ref message, testClient);
            Assert.AreEqual("credsLegit", message);
        }

        [Test]
        public void RemoveUserFromListShouldRemove()
        {
            int preUserCount = Program._users.Count();
            string message = Program.RemoveUserFromList(client.GUID.ToString());
            int postUserCount = Program._users.Count();

            Assert.AreEqual($"User [{client.GUID}] removed from list", message);
            Assert.AreEqual(preUserCount-1, postUserCount);
        }

        [Test]
        public void RemoveUserFromListShouldReturnError()
        {
            Guid guid = Guid.NewGuid();
            string message = Program.RemoveUserFromList(guid.ToString());
            Assert.AreEqual($"Error while finding user to remove from list [{guid}].", message);
        }
        
        [Test]
        public void SetUserInHubReadyShouldReturnTrue()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            hubmodel.MaxSize = 2;
            var hub = Program.UpdateReceivedLobbyInfo(client.GUID.ToString(), hubmodel);

            client.UserModel.IsReady = false;
            bool result = Program.SetThisPlayerReady(client.GUID.ToString(), out hub);
            Assert.AreEqual(true, result);
        }    
        
        [Test]
        public void SetUserInHubReadyShouldReturnFalse()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            var hub = Program.UpdateReceivedLobbyInfo(client.GUID.ToString(), hubmodel);

            client.UserModel.IsReady = true;
            bool result = Program.SetThisPlayerReady(client.GUID.ToString(), out hub);
            Assert.AreEqual(false, result);
        }


        [TearDown]
        public void Teardown()
        {
            Program._hubs.Clear();
            Program._users.Clear();
            client = null;
        }
    }
}
