namespace Tamkeen.Application.DTOs.AI
{
    public class MaintenanceScheduleRequestDto
    {
        public List<HomeDeviceDto> Devices { get; set; }
        public string LastMaintenanceDate { get; set; }
    }
}
