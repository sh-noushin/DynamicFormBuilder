using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;

using FormBuilder.Models.Entities;
using Xunit;

namespace FormBuilder.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _configuration = Substitute.For<IConfiguration>();
            _jwtService = new JwtService(_configuration);
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsTokenString()
        {
            // Arrange
            var user = new UserDto
            {
                Id = "1",
                Username = "testuser",
                Email = "test@example.com",
                Roles = new List<UserRole> { UserRole.User },
                CreatedAt = DateTime.UtcNow
            };
            var roles = new List<string> { "User" };

            _configuration["Jwt:Key"].Returns("test-key-test-key-test-key-test-key-test-key-test-key");
            _configuration["Jwt:Issuer"].Returns("TestIssuer");
            _configuration["Jwt:Audience"].Returns("TestAudience");
            _configuration["Jwt:ExpireMinutes"].Returns("60");

            // Act
            var token = await _jwtService.GenerateTokenAsync(user, roles);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
        }
    }
}
