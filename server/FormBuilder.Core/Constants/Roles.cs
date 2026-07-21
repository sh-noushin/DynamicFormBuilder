namespace FormBuilder.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    // Vendor-level super admin. Belongs to no tenant. Manages the list of
    // customer workspaces and can impersonate any tenant admin for
    // support. Provisioned only via seeding — never by self-signup or
    // by a tenant admin from within the app.
    public const string SuperAdmin = "SuperAdmin";

    public const string AdminOrUser = Admin + "," + User;
}
