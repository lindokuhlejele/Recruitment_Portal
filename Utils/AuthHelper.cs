using System.Web;
using System;
using Recruitment_Portal.Models;
using System.Linq;

public static class AuthHelper
{

    public static Guid GetUserId(HttpSessionStateBase session)
    {
        var value = session["UserId"];

        if (value == null)
            throw new Exception("User session missing");

        if (!Guid.TryParse(value.ToString(), out Guid userId))
            throw new Exception("Invalid GUID in session: " + value);

        return userId;
    }

    public static string GetFirstName(HttpSessionStateBase session)
    {
        var userId = AuthHelper.GetUserId(session);

        using (var db = new RoyalPortalDbEntities())
        {
            var user = db.users.FirstOrDefault(x => x.id == userId);

            return user?.email;
        }
    }
    public static bool IsLoggedIn(HttpSessionStateBase session)
    {
        return session["UserId"] != null;
    }
}