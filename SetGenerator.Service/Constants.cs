namespace SetGenerator.Service
{
    public class Constants
    {
        // selectlist text
        public struct SelectListText
        {
            public const string NoneSelected = "<None>";
        }

        // validation
        public const int MinPassowrdLength = 4;
        public const int MaxAccountNumberLength = 20;

        // email context
        public const string EmailContextRegister = "The following user has just registered";
        public const string EmailContextProfileEdited = "The following user has just edited their profile";
        public const string EmailContextPasswordChanged = "The following user has just changed their password";

        // table column
        public struct UserTableColumn
        {
            public const int KeyId = 15;
            public const int SingerId = 16;
        }

        // user table
        public struct UserTable
        {
            public const int BandId = 1;
            public const int SongId = 2;
            public const int GigId = 3;
            public const int MemberId = 4;
            public const int SetId = 5;
            public const int SetlistId = 6;
        }

        // user table
        public struct Band
        {
            public const int Slugfest = 1;
            public const int TheBeadles = 2;
            public const int StetsonBrothers = 3;
        }
        // Setlist
        public const int MinSongInstrumentationCount = 3;
        public const int MaxDefaultSingerCount = 3;
    }

    // song

    public enum Tempo
    {
        Slow = 1,
        Medium = 2,
        Fast = 3
    }

    public enum Genre
    {
        Rock = 1,
        Country = 2,
        Blues = 3,
        Jazz = 4,
        Gospel = 5,
        Waltz = 6,
        RAndB = 7,
        Surf = 8,
        Ballad = 9
    }
}
