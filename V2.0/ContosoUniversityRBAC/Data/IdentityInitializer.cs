using ContosoUniversityRBAC.Areas.Admin;
using Elfie.Serialization;
using Humanizer.Localisation;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
 
using System.Security.Claims;
using static System.Formats.Asn1.AsnWriter;

namespace ContosoUniversityRBAC.Data
{
    public static class IdentityInitializer
    {


        public static async Task<MyUser?> CreateUser(string username, UserManager<MyUser> _userManager)
        {
            MyUser? myuser=null;
            try
            {
                //myuser = await _userManager.Users.Any(s => s.UserName == username);
                if (! _userManager.Users.Any(s => s.UserName == username))
                {
                   
                    myuser = new MyUser { UserName = $"{username}", Email = $"{username}@tifs.com" };
                    await _userManager.CreateAsync(myuser, "123456");
                    
                }
                else
                {
                    myuser = await _userManager.FindByNameAsync(username);
                }
            }
            catch (Exception ex)
            {
                ILogger logger = _userManager.Logger;
                logger.LogCritical($"####CreateUser Error username:{username},{ex.Message}");
            }
            return myuser;
        }
        public static async Task<IdentityResult?> CreateClaim(string claimname,MyRole role, RoleManager<MyRole> _roleManager)
        {
            IdentityResult  res= new IdentityResult();
            try
            {
                var claims =await  _roleManager.GetClaimsAsync(role);
 
                if (! claims.Any(c => c.Type == ClaimTypes.Role && c.Value == claimname) )
                {
                     res= await _roleManager.AddClaimAsync(role, new Claim(ClaimTypes.Role, claimname));
                
                }
                
            }
            catch (Exception ex)
            {
                ILogger logger = _roleManager.Logger;
                logger.LogCritical($"####CreateroleClaims Error rolename:{role.Name},{claimname},{ex.Message}");

            }
            return res;

        }
        public static async Task<MyRole?> CreateRole(string rolename, RoleManager<MyRole> _roleManager)
        {
            MyRole? role = null;
            try
            {
                if (!_roleManager.Roles.Any(s => s.Name == rolename))
                {
                    role = new MyRole { Name = rolename };
                    await _roleManager.CreateAsync(role);

                }
                else
                {
                    role = await _roleManager.FindByNameAsync(rolename);
                }
            }
            catch (Exception ex)
            {
                ILogger logger = _roleManager.Logger;
                logger.LogCritical($"####Createrole Error rolename:{rolename},{ex.Message}");
                
            }
            return role;
        }
        public static async Task Initialize(UserManager<MyUser> _userManager,RoleManager<MyRole> _roleManager)
      
        {
            MyUser? user;
            MyRole? role;
            user=await CreateUser("admin", _userManager);
            role = await CreateRole("RoleAdmin", _roleManager);
            if (user != null && role !=null && role.Name !=null)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            user=await CreateUser("SuperAdmin", _userManager);
            role =await CreateRole("RoleSuperAdmin", _roleManager);
            if (user != null && role != null && role.Name != null)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }

            for (int i = 0; i < 10; i++) 
                {        
                   await CreateUser($"Test{i}", _userManager);      
                } 
        }

        public static async Task InitializeResource(RoleManager<MyRole> _roleManager,AuthorizationAndMenu _authorizationAndMenu)
        { 
            ILogger logger = _roleManager.Logger;
            try
            {
                // logger.LogInformation($"##### Resource:{ResourceData.Resources.Count.ToString()}");
                MyRole  superrole = await CreateRole($"RoleSuperAdmin", _roleManager);
                MyRole  adminrole = await CreateRole($"RoleAdmin", _roleManager);
                foreach (var resource in _authorizationAndMenu.ResourcesRoles)
                {
                    if (superrole != null)
                    {
                       // await _roleManager.AddClaimAsync(superrole, new Claim(ClaimTypes.Role, resource));
                       await CreateClaim(resource, superrole,_roleManager);
                    }
                    if (resource.Contains("Role") || resource.Contains("User"))
                    {
                        await CreateClaim(resource, adminrole, _roleManager);
                    }
                    else
                    {
                        MyRole? role = await CreateRole($"Role{resource}", _roleManager);


                        if (role != null)
                        {
                            await CreateClaim(resource, role, _roleManager);
                        }
                    }
                       
                   

                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"InitializeResource Error: {ex.Message}");
            }


        }
    }
}
