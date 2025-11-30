using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormBuilder.Core.Services;
using FormBuilder.Models.Entities;
using NSubstitute;
using Xunit;

namespace FormBuilder.Tests.Services
{
    public class FieldValidatorEdgeCasesTests
    {
        [Fact]
        public async Task NullFieldValues_AreHandled_NoErrorsForOptional()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new System.Collections.Generic.List<FormVersionField>
                {
                    new FormVersionField { Name = "optional", IsRequired = false, Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var result = await validator.ValidateAsync(versionId, null);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UnknownFieldNames_AreIgnored()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new System.Collections.Generic.List<FormVersionField>
                {
                    new FormVersionField { Name = "known", IsRequired = false, Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            var payload = new Dictionary<string, string?> { { "unknown", "value" } };
            var result = await validator.ValidateAsync(versionId, payload);
            Assert.Empty(result);
        }

        [Fact]
        public async Task FormVersionNotFound_ReturnsFormLevelError()
        {
            var versionId = Guid.NewGuid();
            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult<FormVersion?>(null));

            var validator = new FieldValidator(repo);
            var result = await validator.ValidateAsync(versionId, new Dictionary<string, string?> { { "a", "b" } });
            Assert.True(result.ContainsKey("__form"));
        }

        [Fact]
        public async Task FileField_IsTreatedLikeOtherFields_PreservesRequiredBehavior()
        {
            var versionId = Guid.NewGuid();
            var version = new FormVersion
            {
                Id = versionId,
                Fields = new System.Collections.Generic.List<FormVersionField>
                {
                    new FormVersionField { Name = "upload", IsRequired = true, Order = 1 }
                }
            };

            var repo = Substitute.For<FormBuilder.Models.Repositories.IFormVersionRepository>();
            repo.GetVersionByIdAsync(versionId).Returns(Task.FromResult(version));

            var validator = new FieldValidator(repo);
            // simulate no token provided
            var result = await validator.ValidateAsync(versionId, new Dictionary<string, string?>());
            Assert.True(result.ContainsKey("upload"));
            Assert.Contains("This field is required.", result["upload"]);
        }
    }
}
