namespace ContosoUniversityRBAC.Areas.Admin.Models
{
    public class AssignClaimData
    {
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
        public bool Assigned { get; set; }
    }
}
