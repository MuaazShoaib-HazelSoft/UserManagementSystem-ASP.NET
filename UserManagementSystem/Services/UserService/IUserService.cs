﻿using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.DTOS.UsersDTO;
using UserManagementSystem.Models;
using UserManagementSystem.Models.ResponseModel;

namespace UserManagementSystem.Services.UserService
{
    /// <summary>
    /// Interface having all
    /// methods of Users.
    /// </summary>
    public interface IUserService
    {
        Task<List<GetUsersWithRolesAndCourses>> GetAllUsers();
        Task<GetUsersWithRolesAndCourses> GetUserById(string Id);
        Task  UpdateUser(string Id,UpdateUserDto updatedUser);
        Task DeleteUser(string Id);
        Task<PaginatedResponse<GetUsersWithRolesAndCourses>> GetPagedUsers(PaginationQueryModel paginationQueryModel);
        Task AssignRoleToUser(string userId,string roleName);
    }
}
