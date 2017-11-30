using System.Text.RegularExpressions;

public class ChatAction {

    public struct StoryAction
    {
        public string CharacterID;
        public string Command;
        public string[] Parameter;
        public MatchCollection Richparamater;

        public string LoopType;
        public SKIPTYPE SkipType;

        public NOWSTATE NowState;
    }

    public struct StoryCharacter
    {
        public string CharacterID;
        public int Orientation;
        public string Name;
        public string Image;
        public string Windows;
        public string Voice;
    }

    public enum SKIPTYPE
    {
        CLICK,
        AUTO,
        TimeAUTO,
        SAMETIME
        
    }
    public enum NOWSTATE
    {
        WAITING,
        DOING,
        DONE
    }
}
