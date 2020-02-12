
namespace QueueService.Api
{
    public class AppSettings
    {

        public string HmacId { get; set; }
        public string HmacSecret { get; set; }

        public int? AllowedDateDriftMinutes { get; set; }
    }
}
