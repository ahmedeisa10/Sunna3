using Tamkeen.Application.DTOs.AI;
namespace Tamkeen.Application.Interfaces.AI
{
    public interface IAIService
    {
        Task<MaintenanceAdviceResponseDto> GetMaintenanceAdviceAsync(string problem);
        Task<MaintenanceScheduleResponseDto> GetMaintenanceScheduleAsync(MaintenanceScheduleRequestDto dto);
    }
}
