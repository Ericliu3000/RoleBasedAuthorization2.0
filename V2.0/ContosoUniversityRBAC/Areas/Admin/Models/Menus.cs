namespace ContosoUniversityRBAC.Areas.Admin.Models
{
    public class Menus : ICloneable
    {
        public string Name { get; set; } 
       
        public string? DisplayName { get; set; }
        public int Order { get; set; } = 0;
        public string Url { get; set; } = "";
        public string Area { get; set; } = "";
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "";
        public string? AllowedRoles { get; set; }
        public bool Isactive { get; set; }

        // 实现深拷贝
        public object Clone()
        {
            return new Menus
            {
                Name = this.Name,
                AllowedRoles = this.AllowedRoles,
                Isactive = this.Isactive,
                DisplayName = this.DisplayName,
                Order = this.Order,
                Url = this.Url,
                Area = this.Area,
                Controller = this.Controller,
                Action = this.Action

            };
        }
    }
}
