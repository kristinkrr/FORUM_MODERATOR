using Microsoft.ML.Data;

namespace MyMvcApp.Models
{
    public class CommentInput
    {
        [LoadColumn(0)]
        public string Text { get; set; } = null!;
    }
}