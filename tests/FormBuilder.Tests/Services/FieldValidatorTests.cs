using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormBuilder.Core.Services;
using FormBuilder.Models.Entities;
using NSubstitute;
using Xunit;

namespace FormBuilder.Tests.Services
{
    public class FieldValidatorTests
    {
        [Fact]
        public async Task RequiredFieldMissing_ReturnsError()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new List<FormVersionField>
                {
                    new FormVersionField { Name = "firstName", IsRequired = true, Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var result = await validator.ValidateAsync(versionId, new Dictionary<string, string?>());

            Assert.True(result.ContainsKey("firstName"));
            Assert.Contains("This field is required.", result["firstName"]);
        }

        [Fact]
        public async Task PatternMismatch_ReturnsError()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new List<FormVersionField>
                {
                    new FormVersionField { Name = "code", IsRequired = true, Validation = "{\"pattern\":\"^[A-Z]{3}$\"}", Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var result = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "code", "ab1" } });

            Assert.True(result.ContainsKey("code"));
            Assert.Contains(result["code"], s => s.Contains("pattern") || s.Contains("match"));
        }

        [Fact]
        public async Task MinMaxLength_ReturnsErrors()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new List<FormVersionField>
                {
                    new FormVersionField { Name = "bio", Validation = "{\"minLength\":5,\"maxLength\":10}", Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            // too short
            var shortResult = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "bio", "abc" } });
            Assert.True(shortResult.ContainsKey("bio"));
            Assert.Contains(shortResult["bio"], s => s.Contains("Minimum length"));

            // too long
            var longValue = new string('x', 20);
            var longResult = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "bio", longValue } });
            Assert.True(longResult.ContainsKey("bio"));
            Assert.Contains(longResult["bio"], s => s.Contains("Maximum length"));
        }

        [Fact]
        public async Task NumericBounds_ReturnsErrors()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new List<FormVersionField>
                {
                    new FormVersionField { Name = "age", Type = FieldType.Number, Validation = "{\"minimum\":18,\"maximum\":65}", Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var tooYoung = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "age", "16" } });
            Assert.True(tooYoung.ContainsKey("age"));
            Assert.Contains(tooYoung["age"], s => s.Contains("Minimum value"));

            var tooOld = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "age", "80" } });
            Assert.True(tooOld.ContainsKey("age"));
            Assert.Contains(tooOld["age"], s => s.Contains("Maximum value"));
        }

        [Fact]
        public async Task AllowedValues_ReturnsError()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new List<FormVersionField>
                {
                    new FormVersionField { Name = "size", Validation = "{\"allowed\":[\"S\",\"M\",\"L\"]}", Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var result = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "size", "XL" } });
            Assert.True(result.ContainsKey("size"));
            Assert.Contains(result["size"], s => s.Contains("allowed" ) || s.Contains("allowed options") || s.Contains("allowed"));
        }

        [Fact]
        public async Task InvalidValidationJson_ReturnsConfigError()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new List<FormVersionField>
                {
                    new FormVersionField { Name = "x", Validation = "{ invalid json }", Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var result = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "x", "val" } });
            Assert.True(result.ContainsKey("x"));
            Assert.Contains(result["x"], s => s.Contains("Invalid validation configuration") || s.Contains("invalid", StringComparison.OrdinalIgnoreCase));
        }
    }
}
