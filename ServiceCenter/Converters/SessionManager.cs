using ServiceCenter.Models;

namespace ServiceCenter
{
    public static class SessionManager
    {
        public static User CurrentUser { get; private set; }

        public static void Login(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsAuthenticated => CurrentUser != null;

        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsMaster => CurrentUser?.Role == UserRole.Master;
        public static bool IsManager => IsMaster;
    }
}
