using Digital_Fridge.Models.Enums;

namespace Digital_Fridge.Models;

public record ProductAnalysisResult(
    string Name,
    string? CategoryName,
    decimal Quantity,
    UnitEnum Unit,
    ProcessingLevel? ProcessingLevel,
    DateOnly? EstimatedExpiration,
    string? Barcode
);
