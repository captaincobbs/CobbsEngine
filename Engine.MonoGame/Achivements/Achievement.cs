using System;

namespace Engine.MonoGame.Achivements
{
    /// <summary>Represents an actual achievement definition</summary>
    public class Achievement
    {
        public string ID { get; set; } = "";

        public string Name { get; set; } = "";
        public string Description { get; set; }

        public Uri IconURL { get; set; }
        public Uri LockedIconURL { get; set; }
        public Uri UnlockedIconURL { get; set; }

        public bool IsHidden { get; set; }
        public bool IsSecret { get; set; }

        public int? Points { get; set; }

        public int DisplayOrder { get; set; }
    }
}
