﻿using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace UserManagementSystem.DTOS.UsersDTO
{
    /// <summary>
    /// Class of Register User Dto
    /// exluding Id.
    /// </summary>
    public class RegisterUserDto
    {

        [Required(ErrorMessage = MessagesConstants.NameRequired)]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = MessagesConstants.EmailRequired)]
        [EmailAddress(ErrorMessage = MessagesConstants.InvalidEmail)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = MessagesConstants.PasswordRequired)]
        [MinLength(5, ErrorMessage = MessagesConstants.InvalidPassword)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = MessagesConstants.AgeRequired)]
        public int? Age { get; set; }
    }
}
