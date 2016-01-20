﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetGenerator.Service
{
    public class Constants
    {
        // validation
        public const int MinPassowrdLength = 4;
        public const int MaxAccountNumberLength = 20;

        // email context
        public const string EmailContextRegister = "The following user has just registered";
        public const string EmailContextProfileEdited = "The following user has just edited their profile";
        public const string EmailContextPasswordChanged = "The following user has just changed their password";

        // user table
        public struct UserTable
        {
            public const int BandId = 1;
            public const int SongId = 2;
            public const int GigId = 3;
            public const int MemberId = 4;
            public const int SetId = 5;
            public const int SetListId = 6;
            public const int UserId = 5;
        }

        // Setlist
        public const int MinSongInstrumentationCount = 3;
        public const int MaxDefaultSingerCount = 3;
    }
}