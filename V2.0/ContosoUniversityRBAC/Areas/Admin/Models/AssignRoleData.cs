namespace ContosoUniversityRBAC.Areas.Admin.Models
{
    public class AssignRoleData
    {
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool Assigned { get; set; }
    }
}
