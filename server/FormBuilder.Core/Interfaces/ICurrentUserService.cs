namespace FormBuilder.Core.Interfaces;

// Surfaces the currently-authenticated caller's tenant so service-layer
// queries can filter by it without reaching for HttpContext directly.
// The implementation lives in FormBuilder.API (where IHttpContextAccessor
// is available) and pulls values from the JWT/ApiKey claims.
public interface ICurrentUserService
{
    // OrganizationId of the current caller. Throws if there is no
    // authenticated caller — every service method calling this expects
    // to be running inside an authenticated request.
    Guid GetOrganizationId();

    // Same as above but returns null when there's no authenticated caller,
    // for the rare code path that runs both authenticated and anonymous.
    Guid? GetOrganizationIdOrNull();
}
