using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using ICT_LAB_Web.Components.Entities;
using ICT_LAB_Web.Components.Services.Interfaces;

using Moq;

using NUnit.Framework;

namespace ICT_LAB_Web.Tests
{
    [TestFixture]
    public class RoomRepositoryTests
    {
        private List<Room> TestData;
        private Mock<IRoomRepository> RoomRepository;

        #region Constructor

        public RoomRepositoryTests()
        {
            this.InitializeData();

            // Mock repo
            RoomRepository = new Mock<IRoomRepository>();

            // Get all rooms
            RoomRepository.Setup(e => e.GetAll()).ReturnsAsync(TestData);

            // Get room by room code
            RoomRepository.Setup(e => e.Get(It.IsAny<string>())).ReturnsAsync((string room) => TestData.FirstOrDefault(q => q.RoomCode == room));

            // Add room
            RoomRepository.Setup(e => e.Add(It.IsAny<Room>())).ReturnsAsync((Room room) => { TestData.Add(room); TestData.FirstOrDefault(q => q.RoomCode == room.RoomCode); return room; });

            // Update room
            RoomRepository.Setup(e => e.Update(It.IsAny<Room>())).ReturnsAsync((Room room) => { var original = TestData.FirstOrDefault(q => q.RoomCode == room.RoomCode); original = room; return room; });

            // Delete room
            RoomRepository.Setup(e => e.Delete(It.IsAny<string>())).ReturnsAsync((string room) => { TestData.Remove(TestData.FirstOrDefault(q => q.RoomCode == room)); return TestData.FirstOrDefault(q => q.RoomCode == room) == null ? true : false; });
        }

        #endregion

        #region Tests
     
        [Test]
        [Order(1)]
        public async Task Get_All()
        {
            // Arrange
            var repo = RoomRepository.Object;

            // Act
            var result = await repo.GetAll();

            // Assert
            var rooms = result.Should().BeAssignableTo<List<Room>>().Subject;

            rooms.Should().NotBeNull();
            rooms.Count().Should().Be(5);
        }

        [Test]
        [Order(2)]
        public async Task Get_By_RoomCode()
        {
            // Arrange
            var repo = RoomRepository.Object;
            var roomCode = "H.5.314";

            // Act
            var result = await repo.Get(roomCode);

            // Assert
            var room = result.Should().BeOfType<Room>().Subject;

            room.Should().NotBeNull();
            room.RoomCode.Should().Be("H.5.314");
            room.StudentCapacity.Should().Be(25);
        }

        [Test]
        [Order(3)]
        public async Task Add()
        {
            // Arrange
            var repo = RoomRepository.Object;
            var newRoom = new Room {
                RoomCode = "WD.01.016",
                StudentCapacity = 35,
                HasComputer = true,
                HasSmartboard = true,
                HasWindows = true
            };

            // Act
            var result = await repo.Add(newRoom);

            // Assert
            var room = result.Should().BeOfType<Room>().Subject;

            room.Should().NotBeNull();
            room.RoomCode.Should().Be("WD.01.016");
            room.StudentCapacity.Should().Be(35);

            TestData.Count.Should().Be(6);
        }

        [Test]
        [Order(4)]
        public async Task Update()
        {
            // Arrange
            var repo = RoomRepository.Object;
            var originalRoom = TestData.FirstOrDefault(q => q.RoomCode == "WD.01.016");
            var roomToUpdate = new Room
            {
                RoomCode = "WD.01.016",
                StudentCapacity = 30,
                HasComputer = true,
                HasSmartboard = true,
                HasWindows = true
            };

            // Act
            var result = await repo.Update(roomToUpdate);

            // Assert
            var room = result.Should().BeOfType<Room>().Subject;

            room.Should().NotBeNull();
            room.RoomCode.Should().Be("WD.01.016");
            room.StudentCapacity.Should().Be(30);
            room.Should().NotBeSameAs(originalRoom);
        }

        [Test]
        [Order(5)]
        public async Task Delete()
        {
            // Arrange
            var repo = RoomRepository.Object;
            var roomToDelete = TestData[0];

            // Act
            var result = await repo.Delete(roomToDelete.RoomCode);

            // Assert
            result.Should().BeTrue();
            TestData.Count.Should().Be(5);
        }

        #endregion

        #region Setup Data

        private void InitializeData()
        {
            var rooms = new List<Room>
            {
                new Room{ RoomCode = "H.4.308", HasComputer = false, HasSmartboard = false, HasWindows = true, StudentCapacity = 25 },
                new Room{ RoomCode = "H.1.306", HasComputer = true, HasSmartboard = true, HasWindows = false, StudentCapacity = 30 },
                new Room{ RoomCode = "H.5.314", HasComputer = true, HasSmartboard = true, HasWindows = true, StudentCapacity = 25 },
                new Room{ RoomCode = "WD.04.204", HasComputer = true, HasSmartboard = true, HasWindows = false, StudentCapacity = 25 },
                new Room{ RoomCode = "WD.04.002", HasComputer = true, HasSmartboard = false, HasWindows = true, StudentCapacity = 35 },
            };

            this.TestData = rooms;
        }

        #endregion

    }
}
