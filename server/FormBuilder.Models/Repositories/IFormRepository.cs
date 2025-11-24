using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IFormRepository
{
    Task<IEnumerable<Form>> GetAllAsync();
    Task<Form?> GetByIdAsync(Guid id);
    Task<Form> CreateAsync(Form form);
    Task<Form?> UpdateAsync(Guid id, Form form);
    Task<bool> DeleteAsync(Guid id);
}
