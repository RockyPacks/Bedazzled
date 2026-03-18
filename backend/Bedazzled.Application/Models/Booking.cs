using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace Bedazzled.Application.Models;

[FirestoreData]
public class Booking
{
    [FirestoreProperty]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name is too long")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Event type is required")]
    public string EventType { get; set; } = string.Empty;

    [FirestoreProperty]
    [Required(ErrorMessage = "Event date is required")]
    public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);

    [FirestoreProperty]
    public string Message { get; set; } = string.Empty;
}
