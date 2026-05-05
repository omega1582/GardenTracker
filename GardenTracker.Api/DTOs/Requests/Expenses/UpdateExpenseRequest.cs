using System.ComponentModel.DataAnnotations;
using GardenTracker.Core.Enums;

namespace GardenTracker.Api.DTOs.Requests.Expenses;

public class UpdateExpenseRequest
{
    public int? BedId { get; set; }

    public int? SupplierId { get; set; }

    [Required]
    public ExpenseCategory Category { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly ExpenseDate { get; set; }
}
