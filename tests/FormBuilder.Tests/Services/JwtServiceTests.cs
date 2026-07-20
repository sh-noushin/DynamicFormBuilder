using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Options;
using FormBuilder.Core.Services;
using FormBuilder.Models.Entities;
using Microsoft.Extensions.Options;
using Xunit;

namespace FormBuilder.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            var options = Options.Create(new JwtOptions
            {
                Key = "test-key-test-key-test-key-test-key-test-key-test-key",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpireMinutes = 60
            });
            _jwtService = new JwtService(options);
        }

        [Fact]
        public void GenerateToken_ReturnsTokenString()
        {
            var user = new UserDto
            {
                Id = "1",
                Username = "testuser",
                Email = "test@example.com",
                Roles = new List<UserRole> { UserRole.User },
                CreatedAt = DateTime.UtcNow
            };
            var roles = new List<string> { "User" };

            var token = _jwtService.GenerateToken(user, roles);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }
    }
}
