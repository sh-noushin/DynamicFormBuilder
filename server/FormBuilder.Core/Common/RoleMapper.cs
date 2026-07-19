using FormBuilder.Models.Entities;

namespace FormBuilder.Core.Common;

public static class RoleMapper
{
    public static List<UserRole> ToEnumRoles(IList<string> stringRoles)
    {
        var enumRoles = new List<UserRole>();
        foreach (var role in stringRoles)
        {
            if (Enum.TryParse<UserRole>(role, out var enumRole))
                enumRoles.Add(enumRole);
        }
        return enumRoles;
    }
}
