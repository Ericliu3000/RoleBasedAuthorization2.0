using AspNetCoreGeneratedDocument;
using ContosoUniversityRBAC.Areas.Admin.Models;
using ContosoUniversityRBAC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContosoUniversityRBAC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RolesController : Controller
    {
        private readonly MyDbContext _context;
        private readonly RoleManager<MyRole> _roleManager;
        private readonly ILogger<RolesController> _logger;
        private readonly AuthorizationAndMenu _authorizationAndMenu;
        public RolesController(MyDbContext context, RoleManager<MyRole> roleManager, ILogger<RolesController> logger,AuthorizationAndMenu authorizationAndMenu)
        {
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
            _authorizationAndMenu = authorizationAndMenu;
        }

        // GET: Roles
         
        [Authorize(Roles = "RolesRead")]
        public async Task<IActionResult> Index(string? sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SortOrder"] = string.IsNullOrEmpty(sortOrder) ? "rolename_desc" : "";
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            IQueryable<MyRole> roleQuery;
            switch (sortOrder)
            {
                case "rolename_desc":
                    roleQuery =  _roleManager.Roles.OrderByDescending(x => x.Name);
                    break;

                default:
                    roleQuery = _roleManager.Roles.OrderBy(x => x.Name);
                    break;
            }
            // var roles = await _roleManager.Roles.ToListAsync();

            int pageSize = 20;
            return View(await PaginatedList<MyRole>.CreateAsync(
                roleQuery.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Roles/Details/5
        [Authorize(Roles = "RolesRead")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var role= await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            else
            {
                RoleInputModel model = new RoleInputModel();

                return View(new RoleInputModel(role));
            }

           
        }

        // GET: Roles/Create
        [Authorize(Roles = "RolesDelete")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "RolesDelete")]
        public async Task<IActionResult> Create(  RoleInputModel roleInputModel)
        {
             

            if (ModelState.IsValid)
            {
                
                try
                {
                     
                    MyRole role = new MyRole { Name = roleInputModel.Name };
                    
                   IdentityResult result = await _roleManager.CreateAsync(role);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", $"Can not create the Role. Reason: {result.Errors.FirstOrDefault().Description}");
                       // ViewData["ConfirmationMessage"] = $"Can not create the Role. Reason: {result.Errors.FirstOrDefault().Description}";
                        

                        return BadRequest(result.ToString()); 
                    }

                    ViewData["ConfirmationMessage"] = "User was Created Successfully!";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception  ex  )
                {
                    //Log the error (uncomment ex variable name and write a log.
                  
                    ModelState.AddModelError("",ex.Message);
                }
            }
            return View(roleInputModel);
        }

        // GET: Roles/Edit/5
        [Authorize(Roles = "RolesWrite")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                MyRole myrole = await _roleManager.FindByIdAsync(id);
                if (myrole == null)
                {
                    return NotFound();
                }
                else
                {
                    await PopulateAssignedClaimDataAsync(myrole);

                    return View( new RoleInputModel(myrole));

                }
            }
            
            
        }
        [Authorize(Roles = "RolesWrite")]
        // POST: Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,  RoleInputModel roleInputModel, string[] selectedClaims)
        {
            if (id != roleInputModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
             
                    MyRole role = await _roleManager.FindByIdAsync(roleInputModel.Id);
                    if (role == null)
                    { return NotFound();                 }
                    else
                    {
                         role.Name= roleInputModel.Name;
                      
                        try
                        {
                             IdentityResult result = await _roleManager.UpdateAsync(role);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", $"Can not save the role. Reason: {result.Errors.FirstOrDefault().Description}");
                                //ViewData["ConfirmationMessage"] = $"Can not create the user. Reason: {result.Errors.FirstOrDefault().Description}";
                                return View(roleInputModel);
                            }
                        }
                        catch (Exception ex) {
                            ModelState.AddModelError("", $"Can not save the role. Reason:{ex.Message}");

                            return View(roleInputModel);
                        }
                        
                    
                     
                        await UpdateRoleClaimsAsync(selectedClaims, role);
                    }
                 
                }
                return RedirectToAction(nameof(Index));
            }



        // GET: Roles/Delete/5
        [Authorize(Roles = "RolesDelete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    MyRole role = await _roleManager.FindByIdAsync(id);
                    if (role == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return View(new RoleInputModel(role));
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return RedirectToAction(nameof(Index));
                }
            }


        }
        [Authorize(Roles = "RolesDelete")]
        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    MyRole role = await _roleManager.FindByIdAsync(id);
                    if (role == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        await _roleManager.DeleteAsync(role);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        private async Task PopulateAssignedClaimDataAsync(MyRole myrole)
        {
            var allResources = _authorizationAndMenu.ResourcesRoles;
            var roleClaims = await _roleManager.GetClaimsAsync(myrole);
            var allClaims = new List<Claim>();
            var viewModel = new List<AssignClaimData>();
            foreach (var resource in allResources)
            {
                allClaims.Add(new Claim(ClaimTypes.Role, resource ));

            }



            foreach (var claim in allClaims)
            {
                viewModel.Add(new AssignClaimData
                {
                    ClaimType = ClaimTypes.Role,
                    ClaimValue = claim.Value,
                    Assigned = roleClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value)

                });
                
            }

 

            ViewData["Claims"] = viewModel;
        }

        private async Task UpdateRoleClaimsAsync(string[] selectedClaims, MyRole  myrole)
        {
            if (selectedClaims == null)
            {
                // userToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }
            var allResources = _authorizationAndMenu.ResourcesRoles;
            var selectedClaimHS = new HashSet<string>(selectedClaims);
            var roleClaims = await _roleManager.GetClaimsAsync(myrole);
            var ClaimsList = roleClaims
                    .Where(claim => claim.Type.Equals(ClaimTypes.Role, StringComparison.Ordinal))
                     .Select(claim => claim.Value)
                    .ToList();

            foreach (var resource in allResources)
            {

                if (selectedClaimHS.Contains(resource))
                {
                    if (!ClaimsList.Contains(resource))
                    {
                         await _roleManager.AddClaimAsync(myrole, new Claim(ClaimTypes.Role, resource));

                    }
                }
                else
                {

                    if (ClaimsList.Contains(resource ))
                    {

                        await _roleManager.RemoveClaimAsync(myrole, new Claim(ClaimTypes.Role, resource ));
                    }
                     
                }
            }
        }
        private bool RoleInputModelExists(string id)
        {
            //return _context.RoleInputModel.Any(e => e.Id == id);
            return _roleManager.Roles.Any(role => role.Id == id);
        }
    }
}
