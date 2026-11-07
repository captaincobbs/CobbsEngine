using System;

namespace Engine.MonoGame.Achivements
{
    public class AchievementUser
    {
        public string ID { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; }

        public Uri AvatarURL { get; set; }
    }
}
