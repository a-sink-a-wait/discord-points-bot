using System;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using Function.Commands;
using Function.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Function.Test.Unit
{
    [TestClass]
    public class CommandIntakeTests
    {
        [TestMethod]
        public async Task Run_ShortCircuits_WhenEventFactoryReturnsNull()
        {
            var mockEventFactory = new Func<JsonDocument, PointsEvent>(document => null);

            var mockGameTimer = new Mock<IGameTimer>();
            mockGameTimer.Setup(timer => timer.Timeout(It.IsAny<string>())).Verifiable();

            var mockEventWriter = new Mock<IEventWriter<PointsEvent>>();
            mockEventWriter.Setup(writer => writer.PushEvents(It.IsAny<PointsEvent>())).Verifiable();

            var structureUnderTest =
                new CommandIntake(mockEventFactory, () => 1, mockGameTimer.Object, mockEventWriter.Object);

            const string CommandPayload = "{}";
            await structureUnderTest.Run(CommandPayload);

            mockGameTimer.Verify(timer => timer.Timeout(It.IsAny<string>()), Times.Never);
            mockEventWriter.Verify(writer => writer.PushEvents(It.IsAny<PointsEvent>()), Times.Never);
        }

        [TestMethod]
        public async Task Run_ShortCircuits_WhenPointsAmountInPayloadIsLessThanZero()
        {
            var mockEventFactory = new Func<JsonDocument, PointsEvent>(document => new PointsEvent{Amount = 0});

            var mockGameTimer = new Mock<IGameTimer>();
            mockGameTimer.Setup(timer => timer.Timeout(It.IsAny<string>())).Verifiable();

            var mockEventWriter = new Mock<IEventWriter<PointsEvent>>();
            mockEventWriter.Setup(writer => writer.PushEvents(It.IsAny<PointsEvent>())).Verifiable();

            var structureUnderTest =
                new CommandIntake(mockEventFactory, () => 1, mockGameTimer.Object, mockEventWriter.Object);

            const string CommandPayload = "{}";
            await structureUnderTest.Run(CommandPayload);

            mockGameTimer.Verify(timer => timer.Timeout(It.IsAny<string>()), Times.Never);
            mockEventWriter.Verify(writer => writer.PushEvents(It.IsAny<PointsEvent>()), Times.Never);
        }

        [TestMethod]
        public async Task Run_ShortCircuits_WhenPointsAreLargerThanConfiguredMaxAmount()
        {
            var mockEventFactory = new Func<JsonDocument, PointsEvent>(document => new PointsEvent { Amount = 30 });

            var mockGameTimer = new Mock<IGameTimer>();
            mockGameTimer.Setup(timer => timer.Timeout(It.IsAny<string>())).Verifiable();

            var mockEventWriter = new Mock<IEventWriter<PointsEvent>>();
            mockEventWriter.Setup(writer => writer.PushEvents(It.IsAny<PointsEvent>())).Verifiable();

            var structureUnderTest =
                new CommandIntake(mockEventFactory, () => 20, mockGameTimer.Object, mockEventWriter.Object);

            const string CommandPayload = "{}";
            await structureUnderTest.Run(CommandPayload);

            mockGameTimer.Verify(timer => timer.Timeout(It.IsAny<string>()), Times.Never);
            mockEventWriter.Verify(writer => writer.PushEvents(It.IsAny<PointsEvent>()), Times.Never);
        }

        [TestMethod]
        public async Task Run_PushesEventsAndTimesOutOriginPlayer_WhenPointsEventIsValid()
        {
            const string OriginPlayerName = "originPlayer";
            var pointsEvent = new PointsEvent
            {
                Amount = 15,
                OriginPlayerId = OriginPlayerName
            };

            var mockEventFactory = new Func<JsonDocument, PointsEvent>(document => pointsEvent);

            var mockGameTimer = new Mock<IGameTimer>();
            mockGameTimer.Setup(timer => timer.Timeout(pointsEvent.OriginPlayerId)).Verifiable();

            var mockEventWriter = new Mock<IEventWriter<PointsEvent>>();
            mockEventWriter.Setup(writer => writer.PushEvents(pointsEvent)).Verifiable();

            var structureUnderTest =
                new CommandIntake(mockEventFactory, () => 20, mockGameTimer.Object, mockEventWriter.Object);

            const string CommandPayload = "{}";
            await structureUnderTest.Run(CommandPayload);

            mockGameTimer.Verify(timer => timer.Timeout(pointsEvent.OriginPlayerId), Times.Once);
            mockEventWriter.Verify(writer => writer.PushEvents(pointsEvent), Times.Once);
        }
    }
}
