using ContosoUniversityRBAC.Areas.Contoso.Models;

namespace ContosoUniversityRBAC.Areas.Contoso.Models.SchoolViewModels
{
    public class InstructorIndexData
    {
        public PaginatedList<Instructor> Instructors { get; set; }
        public PaginatedList<Course> Courses { get; set; }
        public PaginatedList<Enrollment> Enrollments { get; set; }
    }
}
