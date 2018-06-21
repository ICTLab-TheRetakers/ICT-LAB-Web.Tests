using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ReservationRepositoryTests
    {
        private List<Reservation> TestData;
        private Mock<IReservationRepository> ReservationRepository;

        #region Constructor

        public ReservationRepositoryTests()
        {
            this.InitializeData();

            // Mock repo
            ReservationRepository = new Mock<IReservationRepository>();

            // Get all reservation
            ReservationRepository.Setup(e => e.GetAll()).ReturnsAsync(TestData);

            // Get reservation by user id
            ReservationRepository.Setup(e => e.Get(It.IsAny<string>(), null, null)).ReturnsAsync((string id, DateTime? from, DateTime? till) => TestData.Where(q => q.UserId == id).ToList());

            // Get reservation by user id, from and till date
            ReservationRepository.Setup(e => e.Get(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync((string id, DateTime from, DateTime till) => TestData.Where(q => q.UserId == id && q.StartTime >= from && q.EndTime <= till).ToList());

            // Get reservation by id
            ReservationRepository.Setup(e => e.GetById(It.IsAny<int>())).ReturnsAsync((int id) => TestData.FirstOrDefault(q => q.ReservationId == id));

            // Get reservation by room
            ReservationRepository.Setup(e => e.GetByRoom(It.IsAny<string>(), null, null)).ReturnsAsync((string room, DateTime from, DateTime till) => TestData.Where(q => q.RoomCode == room).ToList());

            // Get reservation by room, from and till data
            ReservationRepository.Setup(e => e.GetByRoom(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync((string room, DateTime from, DateTime till) => TestData.Where(q => q.RoomCode == room && q.StartTime >= from && q.EndTime <= till).ToList());

            // Get reservation by start date
            ReservationRepository.Setup(e => e.GetByDate(It.IsAny<DateTime>())).ReturnsAsync((DateTime date) => TestData.Where(q => q.StartTime == date).ToList());

            // Get reservation by room and start date
            ReservationRepository.Setup(e => e.GetByStart(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync((string id, DateTime start) => TestData.FirstOrDefault(q => q.UserId == id && q.StartTime >= start));

            // Add reservation
            ReservationRepository.Setup(e => e.Add(It.IsAny<Reservation>())).ReturnsAsync((Reservation reservation) => { TestData.Add(reservation); TestData.FirstOrDefault(q => q.ReservationId == reservation.ReservationId); return reservation; });

            // Update reservation
            ReservationRepository.Setup(e => e.Update(It.IsAny<Reservation>())).ReturnsAsync((Reservation reservation) => { var original = TestData.FirstOrDefault(q => q.ReservationId == reservation.ReservationId); original = reservation; return reservation; });

            // Delete reservation
            ReservationRepository.Setup(e => e.Delete(It.IsAny<int>())).ReturnsAsync((int id) => { TestData.Remove(TestData.FirstOrDefault(q => q.ReservationId == id)); return TestData.FirstOrDefault(q => q.ReservationId == id) == null ? true : false; });
        }

        #endregion

        #region Tests

        [Test]
        [Order(1)]
        public async Task Get_All()
        {
            // Arrange
            var repo = ReservationRepository.Object;

            // Act
            var result = await repo.GetAll();

            // Assert
            var reservations = result.Should().BeAssignableTo<List<Reservation>>().Subject;

            reservations.Should().NotBeNull();
            reservations.Count().Should().Be(5);
        }

        [Test]
        [Order(2)]
        public async Task Get_By_UserId()
        {
            // Arrange
            var repo = ReservationRepository.Object;
            var userId = "jansmit";

            // Act
            var result = await repo.Get(userId, null, null);

            // Assert
            var reservations = result.Should().BeOfType<List<Reservation>>().Subject;

            reservations.Should().NotBeNull();
            reservations.Count().Should().Be(1);
        }

        [Test]
        [Order(3)]
        public async Task Get_By_UserId_And_Dates()
        {
            // Arrange
            var repo = ReservationRepository.Object;
            var userId = "jansmit";
            var from = DateTime.ParseExact("2018-06-12 09:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var till = from.AddDays(1);

            // Act
            var result = await repo.Get(userId, from, till);

            // Assert
            var reservations = result.Should().BeOfType<List<Reservation>>().Subject;

            reservations.Should().NotBeNull();
            reservations.Count().Should().Be(1);
        }

        [Test]
        [Order(4)]
        public async Task Add()
        {
            // Arrange
            var repo = ReservationRepository.Object;
            var newReservation = new Reservation
            {
                ReservationId = 6,
                RoomCode = "H.4.308",
                UserId = "barackobama",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(4),
                Description = "Work on assignment"
            };

            // Act
            var result = await repo.Add(newReservation);

            // Assert
            var reservation = result.Should().BeOfType<Reservation>().Subject;

            reservation.Should().NotBeNull();
            reservation.RoomCode.Should().Be("H.4.308");
            reservation.UserId.Should().Be("barackobama");
            reservation.ReservationId.Should().Be(6);

            TestData.Count.Should().Be(6);
        }

        [Test]
        [Order(5)]
        public async Task Update()
        {
            // Arrange
            var repo = ReservationRepository.Object;
            var originalReservation = TestData.FirstOrDefault(q => q.RoomCode == "H.4.308");
            var reservationToUpdate = new Reservation
            {
                ReservationId = 6,
                RoomCode = "H.4.308",
                UserId = "barackobama",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(3),
                Description = "Work on assignment"
            };

            // Act
            var result = await repo.Update(reservationToUpdate);

            // Assert
            var reservation = result.Should().BeOfType<Reservation>().Subject;

            reservation.Should().NotBeNull();
            reservation.RoomCode.Should().Be("H.4.308");
            reservation.UserId.Should().Be("barackobama");
            reservation.ReservationId.Should().Be(6);
            reservation.Should().NotBeSameAs(originalReservation);
        }

        [Test]
        [Order(6)]
        public async Task Delete()
        {
            // Arrange
            var repo = ReservationRepository.Object;
            var reservationToDelete = TestData[0];

            // Act
            var result = await repo.Delete(reservationToDelete.ReservationId);

            // Assert
            result.Should().BeTrue();
            TestData.Count.Should().Be(5);
        }

        #endregion

        #region Setup Data

        private void InitializeData()
        {
            var reservations = new List<Reservation>
            {
                new Reservation { ReservationId = 1, UserId = "bobdylan", RoomCode = "H.5.314", Description = "Meeting", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(3) },
                new Reservation { ReservationId = 2, UserId = "barackobama", RoomCode = "WD.01.016", Description = "Meeting with administrators", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) },
                new Reservation { ReservationId = 3, UserId = "spongebobsquarepants", RoomCode = "H.4.312", Description = "Working with team on project", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(4) },
                new Reservation { ReservationId = 4, UserId = "adminhenk", RoomCode = "H.2.203", Description = "Hiding from boss", StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(8) },
                new Reservation { ReservationId = 5, UserId = "jansmit", RoomCode = "WD.04.002", Description = "Secret karaoke", StartTime = DateTime.ParseExact("2018-06-12 11:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), EndTime = DateTime.ParseExact("2018-06-12 13:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) },
            };

            this.TestData = reservations;
        }

        #endregion

    }
}
