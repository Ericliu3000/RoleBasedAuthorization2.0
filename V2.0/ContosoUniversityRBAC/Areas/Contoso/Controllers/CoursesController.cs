using ContosoUniversityRBAC.Areas.Admin;
using ContosoUniversityRBAC.Areas.Contoso.Models;
using ContosoUniversityRBAC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
 
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
namespace ContosoUniversityRBAC.Areas.Contoso.Controllers
{
    //[Authorize(Roles = "CoursesDelete" + "," + "CoursesRead")]
    [Area("Contoso")]
    public class CoursesController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ILogger<CoursesController> _logger;
        private readonly ScopeAuthorizationAndMenu _scopeauthorizationAndMenu;
        private readonly RoleManager<MyRole> _roleManager;
        private readonly UserManager<MyUser> _userManager;


        public CoursesController(MyDbContext context, ILogger<CoursesController> logger,
            ScopeAuthorizationAndMenu scopeauthorizationAndMenu, RoleManager<MyRole> roleManager, UserManager<MyUser> userManager)
        {
            _logger = logger;
            _context = context;
            _scopeauthorizationAndMenu = scopeauthorizationAndMenu;
            _roleManager = roleManager;
            _userManager = userManager;


            // 获取Area名称
            //string areaName = HttpContext.GetRouteValue("area") as string;

            // 获取Controller名称
            //string controllerName = HttpContext.GetRouteValue("controller") as string;

        }

        // GET: Courses
        //[Resource("CoursesRead")]
        [Authorize(Roles = "CoursesRead")]


        //[Authorize(Roles = nameof(Enums.Role.CoursesRead))]
        public async Task<IActionResult> Index(string? sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorizeAction"] = _scopeauthorizationAndMenu.GetAuthorizeAction("Contoso", "Courses");
           // ViewData["AuthorizeController"] = _authorizationAndMenu.GetAuthorizeController(User.Claims);


            var courses = _context.Courses
             .Include(c => c.Department)
             .AsNoTracking();
            switch (sortOrder)
            {
                case "title_desc":
                    courses = courses.OrderByDescending(s => s.Title);
                    break;

                default:
                    courses = courses.OrderBy(s => s.Title);
                    break;
            }
            // return View(await courses.ToListAsync());
            int pageSize = 5;
            return View(await PaginatedList<Course>.CreateAsync(courses.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Courses/Details/5
        //[Resource("Courses.Read")]
        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))
        //[Resource("CoursesRead")]
        //[Authorize(Roles = "CoursesRead" + "," + "DepartmentsRead")]
        [Authorize(Roles = "CoursesRead")]

        public async Task<IActionResult> Details(int? id)
        {
            ViewData["AuthorizeAction"] = _scopeauthorizationAndMenu.GetAuthorizeAction("Contoso", "Courses");
            //ViewData["AuthorizeAction"] = _scopeauthorizationAndMenu.ResourcesMenusAction;

            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(d => d.Department)
                .Include(s => s.Enrollments)
                    .ThenInclude(u => u.Student)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(t => t.Instructor)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))]
        [Authorize(Roles = "CoursesDelete")]
        public IActionResult Create()
        {
            PopulateDepartmentsDropDownList();
            ViewData["AuthorizeAction"] = _scopeauthorizationAndMenu.GetAuthorizeAction("Contoso", "Courses");
         

            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))]
        [Authorize(Roles = "CoursesDelete")]
        public async Task<IActionResult> Create([Bind("CourseID,Title,Credits,DepartmentID")] Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.

                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5

        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))]
        [Authorize(Roles = "CoursesWrite")]
        public async Task<IActionResult> Edit(int? id)
        {
           ViewData["AuthorizeAction"] = _scopeauthorizationAndMenu.GetAuthorizeAction("Contoso", "Courses");
          
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))]
          [Authorize(Roles ="CoursesWrite")]
        public async Task<IActionResult> Edit(int id, [Bind("CourseID,Title,Credits,DepartmentID")] Course course)
        {
            if (id != course.CourseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }
        */
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CoursesWrite")]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseToUpdate = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (await TryUpdateModelAsync<Course>(courseToUpdate,
                "",
                c => c.Credits, c => c.DepartmentID, c => c.Title))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentID);
            return View(courseToUpdate);
        }
        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            // 从数据库上下文查询所有部门，并按名称排序
            var departmentsQuery = from d in _context.Departments
                                   orderby d.Name
                                   select d;

            // 创建SelectList对象，用于在视图中显示下拉列表
            // 参数说明：
            // - 数据源：departmentsQuery.AsNoTracking() - 不跟踪实体更改以提高性能
            // - 数据值字段："DepartmentID" - 下拉列表项的实际值
            // - 数据文本字段："Name" - 下拉列表项显示的文本
            // - 默认选中项：selectedDepartment - 可选参数，指定默认选中的部门
            ViewBag.DepartmentID = new SelectList(departmentsQuery.AsNoTracking(), "DepartmentID", "Name", selectedDepartment);
        }

        // GET: Courses/Delete/5
        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))]
        [Authorize(Roles = "CoursesDelete")]
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            ViewData["AuthorizeAction"] = _scopeauthorizationAndMenu.GetAuthorizeAction("Contoso", "Courses");
       

            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        /// [Authorize(Roles = nameof(Enums.Role.CoursesRead))]
        [Authorize(Roles = "CoursesDelete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id, saveChangesError = true });
            }

        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }

        //ViewData["Authorize"]= dictionaryAuthorize;
       
        }
    
}
