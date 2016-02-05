namespace SetGenerator.Domain.Entities
{
    public class SongMemberInstrumentMatch : EntityBase
    {
        public virtual Band Band { get; set; }
        public virtual Song Song { get; set; }
        public virtual Song MatchingSong { get; set; }
    }
}
