using ThriftShop.Models;

namespace ThriftShop.Common
{
    public static class SessionExtensions
    {
        public const string SessionUserKey = "CurrentUser";
        public const string SessionRoleKey = "UserRole";
        public const string SessionEmailKey = "UserEmail";
        public const string SessionNameKey = "UserName";
        public const string SessionIdKey = "UserId";
        public const string SessionAvatarKey = "UserAvatar";

        public static void SetUser(this ISession session, User user)
        {
            session.SetInt32(SessionIdKey, user.Id);
            session.SetString(SessionEmailKey, user.Email);
            session.SetString(SessionNameKey, user.Name);
            session.SetString(SessionRoleKey, user.Role);
            session.SetString(SessionAvatarKey, user.Avatar ?? "");
        }

        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetInt32(SessionIdKey).HasValue;
        }

        public static bool IsAdmin(this ISession session)
        {
            return session.GetString(SessionRoleKey) == "admin" || session.GetString(SessionEmailKey) == "admin@thriftshop.vn";
        }

        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32(SessionIdKey);
        }

        public static string? GetUserEmail(this ISession session)
        {
            return session.GetString(SessionEmailKey);
        }

        public static string? GetUserName(this ISession session)
        {
            return session.GetString(SessionNameKey);
        }

        public static string? GetUserRole(this ISession session)
        {
            return session.GetString(SessionRoleKey);
        }

        public static string? GetUserAvatar(this ISession session)
        {
            return session.GetString(SessionAvatarKey);
        }

        public static void ClearUser(this ISession session)
        {
            session.Clear();
        }
    }
}
