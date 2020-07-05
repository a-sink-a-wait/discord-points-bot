using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PointsBot.Infrastructure.Commands;

namespace Infrastructure.Test.Unit.Commands
{
    [TestClass]
    public class CommandSenderTests
    {
        [TestMethod]
        public async Task SendAdd_CallsSendAsync_WithMessage()
        {
            var mockTopicClient = new Mock<ITopicClient>();
            var mockQueueClient = new Mock<IQueueClient>();
            mockQueueClient.Setup(queue => queue.SendAsync(It.IsAny<Message>()));

            var structureUnderTest = new CommandSender(mockQueueClient.Object, mockTopicClient.Object);
            await structureUnderTest.SendAdd("somePlayerId", "someTarget", 0, "Test");

            mockQueueClient.Verify(client => client.SendAsync(It.IsAny<Message>()));
        }

        [TestMethod]
        public async Task SendRemove_CallsSendAsync_WithMessage()
        {
            var mockTopicClient = new Mock<ITopicClient>();
            var mockQueueClient = new Mock<IQueueClient>();
            mockQueueClient.Setup(queue => queue.SendAsync(It.IsAny<Message>()));

            var structureUnderTest = new CommandSender(mockQueueClient.Object, mockTopicClient.Object);
            await structureUnderTest.SendRemove("somePlayerId", "someTarget", 0, "Test");

            mockQueueClient.Verify(client => client.SendAsync(It.IsAny<Message>()));
        }

        [TestMethod]
        public async Task SendRemove_CallsSendToSubscription_WithMessageToWarCounter()
        {
            var mockTopicClient = new Mock<ITopicClient>();
            var mockQueueClient = new Mock<IQueueClient>();

            mockTopicClient.Setup(topicClient => topicClient.SendAsync(It.Is<Message>(message => message.To == "WarCounter")));

            var structureUnderTest = new CommandSender(mockQueueClient.Object, mockTopicClient.Object);
            await structureUnderTest.SendRemove("somePlayerId", "someTarget", 0, "Test");

            mockTopicClient.Verify(client => client.SendAsync(It.Is<Message>(message => message.To == "WarCounter")));
        }
    }
}
