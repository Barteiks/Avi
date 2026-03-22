namespace Avi.face
{
    public class Emotions
    {

        public Dictionary<Emotion, FacePose> Create()
        {
            // Populate both left and right fields. Do not "mirror" logic here;
            // poses carry independent values for left and right.
            return new()
            {
                [Emotion.Neutral] = new FacePose
                {

                },
                [Emotion.Confused] = new FacePose
                {
                    LeftBrowOpacity = 1,
                    LeftBrowRotation = -15,
                    LeftBrowY = 25,

                    RightBrowOpacity = 1,
                    RightBrowRotation = -15,
                    RightBrowY = 25,
                },
                [Emotion.Happy] = new FacePose
                {
                    LeftEyeHappy = true,

                    RightEyeHappy = true,
                },
                [Emotion.Scared] = new FacePose
                {
                    LeftEyeEmpty = true,

                    RightEyeEmpty = true,
                },
                [Emotion.VeryScared] = new FacePose
                {
                    LeftEyeEmpty = true,
                    LeftEyeScared = true,

                    RightEyeEmpty = true,
                    RightEyeScared = true,
                },
                [Emotion.Annoyed] = new FacePose
                {
                    LeftBrowY = 100,
                    LeftBrowOpacity = 1,
                    LeftBrowCut = true,

                    RightBrowY = 100,
                    RightBrowOpacity = 1,
                    RightBrowCut = true,
                },
                [Emotion.Afraid] = new FacePose
                {
                    LeftBrowX = -15,
                    LeftBrowY = 55,
                    LeftBrowOpacity = 1,
                    LeftBrowRotation = -20,
                    LeftEyeEmpty = true,
                    LeftEyeScared = true,
                    LeftBrowCut = true,

                    RightBrowX = 15,
                    RightBrowY = 55,
                    RightBrowOpacity = 1,
                    RightBrowRotation = 20,
                    RightEyeEmpty = true,
                    RightEyeScared = true,
                    RightBrowCut = true,

                },
                [Emotion.Sad] = new FacePose
                {
                    LeftBrowX = -15,
                    LeftBrowY = 30,
                    LeftBrowRotation = -20,
                    LeftBrowOpacity = 1,

                    RightBrowX = 15,
                    RightBrowY = 30,
                    RightBrowRotation = 20,
                    RightBrowOpacity = 1,
                },
                [Emotion.Angry] = new FacePose
                {
                    LeftBrowX = 15,
                    LeftBrowY = 40,
                    LeftBrowRotation = 30,
                    LeftBrowOpacity = 1,

                    RightBrowX = -15,
                    RightBrowY = 40,
                    RightBrowRotation = -30,
                    RightBrowOpacity = 1,
                },
                [Emotion.ForcedSmile] = new FacePose
                {
                    LeftBrowX = -15,
                    LeftBrowY = 80,
                    LeftBrowRotation = -20,
                    LeftBrowOpacity = 1,
                    LeftEyeHappy = true,
                    LeftBrowCut = true,

                    RightBrowX = 15,
                    RightBrowY = 80,
                    RightBrowRotation = 20,
                    RightBrowOpacity = 1,
                    RightEyeHappy = true,
                    RightBrowCut = true,
                },

            };
        }
    }
}
