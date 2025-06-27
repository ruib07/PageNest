namespace PageNest.Application.Shared.DTOs;

public static class ResponsesDTO
{
    public record Creation(string Message, Guid Id);
    public record Error(string Message, int StatusCode);
    public record PaymentResponse(string Message, Guid PaymentId, string StripePaymentIntentId);
}
