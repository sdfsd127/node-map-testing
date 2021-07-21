namespace TTR
{
    namespace TTR_Trains
    {
        public enum TRAIN_COLOUR
        {
            ANY = 0,
            Red = 1,
            Green = 2,
            Blue = 3,
            Purple = 4,
            Black = 5,
            White = 6,
            Yellow = 7,
            Orange = 8,
            Grey = 9
        }

        public static class TTR_TrainSetupInfo
        {
            public static TRAIN_COLOUR[] cardColours = new TRAIN_COLOUR[9] { TRAIN_COLOUR.ANY, TRAIN_COLOUR.Red, TRAIN_COLOUR.Green, TRAIN_COLOUR.Blue, TRAIN_COLOUR.Purple,
                                                                         TRAIN_COLOUR.Black, TRAIN_COLOUR.White, TRAIN_COLOUR.Yellow, TRAIN_COLOUR.Orange };

            public static TRAIN_COLOUR[] connectionColours = new TRAIN_COLOUR[9] { TRAIN_COLOUR.Red, TRAIN_COLOUR.Green, TRAIN_COLOUR.Blue, TRAIN_COLOUR.Purple,
                                                                               TRAIN_COLOUR.Black, TRAIN_COLOUR.White, TRAIN_COLOUR.Yellow, TRAIN_COLOUR.Orange, TRAIN_COLOUR.Grey };
        }
    }
}
