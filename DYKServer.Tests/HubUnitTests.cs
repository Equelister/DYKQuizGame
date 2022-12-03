using DYKServer.Net;
using DYKShared.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DYKServer.Tests
{
    public class HubUnitTests
    {
        public Client client;

        [SetUp]
        public void Setup()
        {
            client = new Client();
            client.GUID = Guid.NewGuid();
            Program._users.Add(client);
        }

        [Test]
        public void AddUserToNotExistingHubShouldReturnNull()
        {
            var result = Program.AddUserToHub(99999999, Guid.NewGuid().ToString());
            Assert.IsNull(result);
        }

        [Test]
        public void AddUserToNewHubShouldReturnTrue()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            var result = Program.UpdateReceivedLobbyInfo(client.GUID.ToString(), hubmodel);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ExceedHubLimitReturnNull()
        {
            Program._hubs.Clear();
            HubModel hubmodel = null;
            Hub result = null;
            Client testClient = new Client();
            testClient.GUID = Guid.NewGuid();
            Program._users.Add(testClient);
            for (int i = 0; i < 9050; i++)
            {
                hubmodel = new HubModel("testHub", 0, null);
                result = Program.UpdateReceivedLobbyInfo(testClient.GUID.ToString(), hubmodel);
                if (i == 0)
                {
                    Assert.IsNotNull(result);
                }
                else if (i == 1)
                {
                    Assert.IsNotNull(result);
                }
                else if (i == 10)
                {
                    Assert.IsNotNull(result);
                }
                else if (i == 8998)
                {
                    Assert.IsNotNull(result);
                }
                else if (i == 8999)
                {
                    Assert.IsNotNull(result);
                }
                else if (i == 9000)
                {
                    Assert.IsNull(result);
                }
            }
            Assert.IsNull(result);
        }

        [Test]
        public void AddUserToExistingHubShouldReturnTrue()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            hubmodel.MaxSize = 2;
            Program.UpdateReceivedLobbyInfo(client.GUID.ToString(), hubmodel);
            Hub hub = Program._hubs.Where(x => x.HubModel.Equals(hubmodel)).FirstOrDefault();

            Client testClient = new Client();
            Program._users.Add(testClient);
            testClient.GUID = Guid.NewGuid();
            bool result = hub.AddClient(testClient);

            Assert.IsTrue(result);
            Assert.AreEqual(2, hub.Users.Count);
        }

        [Test]
        public void UpdateExistingHubShouldUpdate()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            hubmodel.MaxSize = 2;
            Program.UpdateReceivedLobbyInfo(client.GUID.ToString(), hubmodel);
            Hub hub = Program._hubs.Where(x => x.HubModel.Equals(hubmodel)).FirstOrDefault();

            hubmodel.Name = "updatedHub";
            Program.UpdateReceivedLobbyInfo(client.GUID.ToString(), hubmodel);

            Assert.AreEqual("updatedHub", hub.HubModel.Name);
        }

        [Test]
        public void AddNotExistingUserToNewHubShouldReturnFalse()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            var result = Program.UpdateReceivedLobbyInfo(Guid.NewGuid().ToString(), hubmodel);
            Assert.IsNull(result);
        }

        [Test]
        public void IsEverybodyEndedGameInHubShouldReturnTrue()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            hubmodel.MaxSize = 8;
            Hub hub = new Hub(hubmodel);
            hub.Users = new System.Collections.Generic.List<Client>();
            for (int i = 0; i < hubmodel.MaxSize; i++)
            {
                Client client = new Client();
                client.UserModel = new UserModel("testUserModel" + i);
                client.UserModel.IsReady = true;
                Program._users.Add(client);
                hub.AddClient(client);
            }

            var result = Program.IsEverybodyInHubEndedGame(hub);
            Assert.IsTrue(result);
        }

        [Test]
        public void IsEverybodyEndedGameInHubShouldReturnFalse2()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            hubmodel.MaxSize = 8;
            Hub hub = new Hub(hubmodel);
            hub.Users = new System.Collections.Generic.List<Client>();
            for (int i = 0; i < hubmodel.MaxSize; i++)
            {
                Client client = new Client();
                client.UserModel = new UserModel("testUserModel" + i);
                if (i < 6)
                {
                    client.UserModel.IsReady = true;
                }
                Program._users.Add(client);
                hub.AddClient(client);
            }

            var result = Program.IsEverybodyInHubEndedGame(hub);
            Assert.IsFalse(result);
        }

        [Test]
        public void IsEverybodyEndedGameInHubShouldReturnFalse()
        {
            HubModel hubmodel = new HubModel("testHub", 0, null);
            hubmodel.MaxSize = 8;
            Hub hub = new Hub(hubmodel);
            hub.Users = new System.Collections.Generic.List<Client>();
            for (int i = 0; i < hubmodel.MaxSize; i++)
            {
                Client client = new Client();
                client.UserModel = new UserModel("testUserModel" + i);
                Program._users.Add(client);
                hub.AddClient(client);
            }

            var result = Program.IsEverybodyInHubEndedGame(hub);
            Assert.IsFalse(result);
        }

        [Test]
        public void FilterPublicHubsAndTransferListtoJsonAndBackShouldReturnTrue()
        {
            List<Hub> hubList = new List<Hub>();
            List<HubModel> hmList = new List<HubModel>();
            for (int i = 0; i < 5; i++)
            {
                HubModel hm = new HubModel();
                hm.Name = "testHub" + 1;
                hm.JoinCode = 1000 + i;
                hm.Category = null;
                hm.MaxSize = 8;
                hm.IsGameStarted = false;
                hm.IsPrivate = false;
                hm.Users = new List<UserModel>();
                hmList.Add(hm);
            }

            foreach (var hubmodel in hmList)
            {
                Hub hub = new Hub(hubmodel);
                hubList.Add(hub);
            }

            var json = Hub.PublicLobbiesToSendCreateJson(hubList);
            var listFromJson = HubModel.JsonListToHubModelList(json);

            for (int i = 0; i < listFromJson.Count; i++)
            {
                Assert.AreEqual(hmList.ElementAt(i).JoinCode, listFromJson.ElementAt(i).JoinCode);
                Assert.AreEqual(hmList.ElementAt(i).Name, listFromJson.ElementAt(i).Name);
                Assert.AreEqual(hmList.ElementAt(i).IsPrivate, listFromJson.ElementAt(i).IsPrivate);
                Assert.AreEqual(hmList.ElementAt(i).Users.Count, listFromJson.ElementAt(i).Users.Count);
            }
        }

        [Test]
        public void FilterPublicHubsAndTransferListtoJsonAndBackShouldReturnTwoLess()
        {
            List<Hub> hubList = new List<Hub>();
            List<HubModel> hmList = new List<HubModel>();
            for (int i = 0; i < 5; i++)
            {
                HubModel hm = new HubModel();
                hm.Name = "testHub" + 1;
                hm.JoinCode = 1000 + i;
                hm.Category = null;
                hm.MaxSize = 8;
                hm.IsGameStarted = false;
                hm.IsPrivate = false;
                hm.Users = new List<UserModel>();
                hmList.Add(hm);
            }
            hmList.ElementAt(1).IsPrivate = true;
            hmList.ElementAt(2).IsPrivate = true;

            foreach (var hubmodel in hmList)
            {
                Hub hub = new Hub(hubmodel);
                hubList.Add(hub);
            }

            var json = Hub.PublicLobbiesToSendCreateJson(hubList);
            var listFromJson = HubModel.JsonListToHubModelList(json);

            Assert.AreEqual(hmList.Count - 2, listFromJson.Count);
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