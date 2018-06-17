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
    public class UserRepositoryTests
    {
        private List<User> TestData;
        private Mock<IUserRepository> UserRepository;

        #region Constructor

        public UserRepositoryTests()
        {
            this.InitializeData();

            // Mock repo
            UserRepository = new Mock<IUserRepository>();

            // Get all users
            UserRepository.Setup(e => e.GetAllUsers()).ReturnsAsync(TestData);

            // Get user by id
            UserRepository.Setup(e => e.Get(It.IsAny<string>())).ReturnsAsync((string id) => TestData.FirstOrDefault(q => q.UserId == id));

            // Get user by email
            UserRepository.Setup(e => e.GetByEmail(It.IsAny<string>())).ReturnsAsync((string email) => TestData.FirstOrDefault(q => q.Email == email));

            // Add user
            UserRepository.Setup(e => e.Add(It.IsAny<User>())).ReturnsAsync((User user) => { TestData.Add(user); TestData.FirstOrDefault(q => q.UserId == user.UserId); return user; });

            // Update user
            UserRepository.Setup(e => e.Update(It.IsAny<User>())).ReturnsAsync((User user) => { var original = TestData.FirstOrDefault(q => q.UserId == user.UserId); original = user; return user; });

            // Delete user
            UserRepository.Setup(e => e.Delete(It.IsAny<string>())).ReturnsAsync((string id) => { TestData.Remove(TestData.FirstOrDefault(q => q.UserId == id)); return TestData.FirstOrDefault(q => q.UserId == id) == null ? true : false; });
        }

        #endregion

        #region Tests

        [Test]
        [Order(1)]
        public async Task Get_All()
        {
            // Arrange
            var repo = UserRepository.Object;

            // Act
            var result = await repo.GetAllUsers();

            // Assert
            var users = result.Should().BeAssignableTo<List<User>>().Subject;

            users.Should().NotBeNull();
            users.Count().Should().Be(5);
        }

        [Test]
        [Order(2)]
        public async Task Get_By_Id()
        {
            // Arrange
            var repo = UserRepository.Object;
            var userId = "barackobama";

            // Act
            var result = await repo.Get(userId);

            // Assert
            var user = result.Should().BeOfType<User>().Subject;

            user.Should().NotBeNull();
            user.UserId.Should().Be("barackobama");
            user.Role.Should().Be(1);
        }

        [Test]
        [Order(3)]
        public async Task Get_By_Email()
        {
            // Arrange
            var repo = UserRepository.Object;
            var email = "sponge@email.com";

            // Act
            var result = await repo.GetByEmail(email);

            // Assert
            var user = result.Should().BeOfType<User>().Subject;

            user.Should().NotBeNull();
            user.Email.Should().Be("sponge@email.com");
            user.FirstName.Should().Be("Spongebob");
        }

        [Test]
        [Order(4)]
        public async Task Add()
        {
            // Arrange
            var repo = UserRepository.Object;
            var newUser = new User
            {
                UserId = "tonystark01",
                FirstName = "Tony",
                LastName = "Stark",
                Email = "ironman@email.com",
                Password = "tonystark99",
                Role = 2,
                Picture = ""
            };

            // Act
            var result = await repo.Add(newUser);

            // Assert
            var user = result.Should().BeOfType<User>().Subject;

            user.Should().NotBeNull();
            user.Email.Should().Be("ironman@email.com");
            user.LastName.Should().Be("Stark");

            TestData.Count.Should().Be(6);
        }

        [Test]
        [Order(5)]
        public async Task Update()
        {
            // Arrange
            var repo = UserRepository.Object;
            var originalUser = TestData.FirstOrDefault(q => q.UserId == "tonystark01");
            var userToUpdate = new User
            {
                UserId = "tonystark01",
                FirstName = "Tony",
                LastName = "Stark",
                Email = "tony.ironman@email.com",
                Password = "tonystark99",
                Role = 2,
                Picture = ""
            };

            // Act
            var result = await repo.Update(userToUpdate);

            // Assert
            var user = result.Should().BeOfType<User>().Subject;

            user.Should().NotBeNull();
            user.UserId.Should().Be("tonystark01");
            user.Email.Should().Be("tony.ironman@email.com");
            user.Should().NotBeSameAs(originalUser);
        }

        [Test]
        [Order(6)]
        public async Task Delete()
        {
            // Arrange
            var repo = UserRepository.Object;
            var userToDelete = TestData[0];

            // Act
            var result = await repo.Delete(userToDelete.UserId);

            // Assert
            result.Should().BeTrue();
            TestData.Count.Should().Be(5);
        }

        #endregion

        #region Setup Data

        private void InitializeData()
        {
            var users = new List<User>
            {
                new User{ FirstName = "Bob", LastName = "Dylan", Email = "rockstar@email.com", Password = "bobdylan99", UserId = "bobdylan", Role = 3, Picture = "" },
                new User{ FirstName = "Spongebob", LastName = "Squarepants", Email = "sponge@email.com", Password = "bikini12", UserId = "spongebobsquarepant", Role = 2, Picture = "" },
                new User{ FirstName = "`Barack", LastName = "Obama", Email = "president2008@email.com", Password = "mrpresident08", UserId = "barackobama", Role = 1, Picture = "" },
                new User{ FirstName = "Henk", LastName = "van de Laan", Email = "henk.admin@email.com", Password = "gekkehenkie", UserId = "adminhenk", Role = 1, Picture = "" },
                new User{ FirstName = "Jan", LastName = "Smit", Email = "jan.smit@email.com", Password = "jansmit01", UserId = "jansmit", Role = 3, Picture = "" },
            };

            this.TestData = users;
        }

        #endregion

    }
}
