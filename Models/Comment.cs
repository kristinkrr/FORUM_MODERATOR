using System;
using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models
{
    public enum CommentStatus
    {
        Pending,
        Approved,
        Flagged
    }

    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Връзка към User
        public int UserId { get; set; }
        public User User { get; set; } = null!;



        // Статус на коментара
        public CommentStatus Status { get; set; } = CommentStatus.Pending;
    }
}