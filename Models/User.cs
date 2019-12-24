using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Column("FirstName", TypeName = "VARCHAR(45)")]
        public string FirstName { get; set; }

        [Required]
        [Column("LastName", TypeName = "VARCHAR(45)")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Column("Email", TypeName = "VARCHAR(45)")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Column("Password", TypeName = "VARCHAR(255)")]
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<Transaction> CreatedTransaction { get; set; }

        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm { get; set; }
    }
}