namespace GameUser.Features.ApiComminicators.Models
{
    public class EnterPortalRequest
    {
        public int ObCustId { get; set; }

        public string SiteId { get; set; }

        public string CharacterName { get; set; }

        public string Seq { get; set; }

        public bool FirstLogin { get; set; }
    }
}