using System;

namespace Project_Pineapplesummer.Modules.Services.Classes
{
    public class BanData
    {
        public int CaseId { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public ulong ModeratorId { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public DateTime ExpirationTime { get; set; }

        public RevokeClass Revoke = new RevokeClass();

        public class RevokeClass
        {
            public ulong ModeratorId { get; set; }
            public DateTime Time { get; set; }
            public string Reason { get; set; }
            public bool IsRevoked { get; set; }
        }

        //public ulong RevokeModeratorId {get; set;}
        //public DateTime RevokeTime { get; set; }
        //public string RevokeReason { get; set; }
        //public bool RevokeIsRevoked { get; set; }

    }

}
