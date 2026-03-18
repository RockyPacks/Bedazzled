using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace Bedazzled.Application.Models;

[FirestoreData]
public class ContactMessage
{
    [FirestoreProperty]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Subject is required")]
    public string Subject { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; } = string.Empty;
}
