using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Services;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;
using FormBuilder.Models.Exceptions;

namespace FormBuilder.Tests.Services
{
    public class UserServiceTests
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IOrganizationRepository _orgs;
        private readonly UserService _userService;
        // Fixed org so tests can create Users with matching OrganizationId
        // and pass the tenant-check that GetById / Update / Delete now do.
        private static readonly Guid TestOrgId = Guid.NewGuid();

        public UserServiceTests()
        {
            _userManager = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _roleManager = Substitute.For<RoleManager<IdentityRole>>(
                Substitute.For<IRoleStore<IdentityRole>>(), null, null, null, null);
            _currentUser = Substitute.For<ICurrentUserService>();
            _currentUser.GetOrganizationId().Returns(TestOrgId);
            _currentUser.GetOrganizationIdOrNull().Returns(TestOrgId);
            _orgs = Substitute.For<IOrganizationRepository>();
            _userService = new UserService(_userManager, _roleManager, _currentUser, _orgs);
        }

        [Fact]
        public async Task RegisterUserAsync_ThrowsArgumentNullException_WhenRegisterDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.RegisterUserAsync(null));
        }

        [Fact]
        public async Task RegisterUserAsync_ThrowsDuplicateUsernameException_WhenUsernameExists()
        {
            var dto = new RegisterUserDto { Username = "user", Email = "email@test.com", Password = "pass", Role = UserRole.User };
            _userManager.FindByNameAsync(dto.Username).Returns(new User());
            await Assert.ThrowsAsync<DuplicateUsernameException>(() => _userService.RegisterUserAsync(dto));
        }

        [Fact]
        public async Task RegisterUserAsync_ThrowsDuplicateEmailException_WhenEmailExists()
        {
            var dto = new RegisterUserDto { Username = "user", Email = "email@test.com", Password = "pass", Role = UserRole.User };
            _userManager.FindByNameAsync(dto.Username).Returns((User)null);
            _userManager.FindByEmailAsync(dto.Email).Returns(new User());
            await Assert.ThrowsAsync<DuplicateEmailException>(() => _userService.RegisterUserAsync(dto));
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsUserDto_WhenSuccess()
        {
            var dto = new RegisterUserDto { Username = "user", Email = "email@test.com", Password = "pass", Role = UserRole.User };
            _userManager.FindByNameAsync(dto.Username).Returns((User)null);
            _userManager.FindByEmailAsync(dto.Email).Returns((User)null);
            _userManager.CreateAsync(Arg.Any<User>(), dto.Password).Returns(IdentityResult.Success);
            _roleManager.RoleExistsAsync(dto.Role.ToString()).Returns(true);
            _userManager.AddToRoleAsync(Arg.Any<User>(), dto.Role.ToString()).Returns(IdentityResult.Success);
            _userManager.GetRolesAsync(Arg.Any<User>()).Returns(new List<string> { dto.Role.ToString() });

            var result = await _userService.RegisterUserAsync(dto);
            Assert.NotNull(result);
            Assert.Equal(dto.Username, result.Username);
            Assert.Equal(dto.Email, result.Email);
            Assert.Contains(UserRole.User, result.Roles);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsUserDtos()
        {
            var user = new User { Id = "1", UserName = "user", Email = "email@test.com", OrganizationId = TestOrgId };
            var users = new List<User> { user };
            _userManager.Users.Returns(users.AsQueryable());
            _roleManager.Roles.Returns(new List<IdentityRole> { new IdentityRole("User") }.AsQueryable());
            _userManager.GetUsersInRoleAsync("User").Returns(new List<User> { user });

            var result = await _userService.GetUsersAsync();
            Assert.Single(result);
            Assert.Equal("user", result.First().Username);
            Assert.Contains(UserRole.User, result.First().Roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetUserByIdAsync(null));
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsUserNotFoundException_WhenUserNotFound()
        {
            _userManager.FindByIdAsync("1").Returns((User)null);
            await Assert.ThrowsAsync<UserNotFoundException>(() => _userService.GetUserByIdAsync("1"));
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserDto_WhenFound()
        {
            var user = new User { Id = "1", UserName = "user", Email = "email@test.com", OrganizationId = TestOrgId };
            _userManager.FindByIdAsync("1").Returns(user);
            _userManager.GetRolesAsync(user).Returns(new List<string> { "User" });

            var result = await _userService.GetUserByIdAsync("1");
            Assert.NotNull(result);
            Assert.Equal("user", result.Username);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateUserAsync(null, new UpdateUserDto()));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserAsync("1", null));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsUserNotFoundException_WhenUserNotFound()
        {
            _userManager.FindByIdAsync("1").Returns((User)null);
            await Assert.ThrowsAsync<UserNotFoundException>(() => _userService.UpdateUserAsync("1", new UpdateUserDto()));
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsArgumentException_WhenIdIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.DeleteUserAsync(null));
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsUserNotFoundException_WhenUserNotFound()
        {
            _userManager.FindByIdAsync("1").Returns((User)null);
            await Assert.ThrowsAsync<UserNotFoundException>(() => _userService.DeleteUserAsync("1"));
        }
    }
}
