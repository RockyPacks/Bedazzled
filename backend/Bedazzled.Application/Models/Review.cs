using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace Bedazzled.Application.Models;

[FirestoreData]
public class Review
{
    [FirestoreProperty]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required]
    public string Author { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required]
    public string Content { get; set; } = string.Empty;

    [FirestoreProperty]
    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [FirestoreProperty]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [FirestoreProperty]
    public string Subtitle { get; set; } = string.Empty;
}
