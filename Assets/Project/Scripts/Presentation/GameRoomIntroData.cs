using UnityEngine;

namespace CardFramework.Presentation {
    public class GameRoomIntroData : ScriptableObject {
        [Header("Room Identity")]
        public string roomTitle = "Cards Game Room";
        [TextArea] public string roomDescription = "Choose a game to start playing.";

        [Header("Hero Visual")]
        public Sprite heroSprite;
        public Sprite[] heroFrames;
        public bool useSpriteAnimation;
        public float animationFrameRate = 0.12f;

        [Header("Room Actions")]
        public GameRoomOptionData[] options;
    }

    [System.Serializable]
    public class GameRoomOptionData {
        public string optionId;
        public string label;
        public Sprite icon;
        public string description;
        public Color accentColor = Color.white;
    }
}
