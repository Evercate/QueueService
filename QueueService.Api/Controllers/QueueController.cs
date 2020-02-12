using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QueueService.Api.Model;
using QueueService.DAL;

namespace QueueService.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class QueueController : ControllerBase
	{
		private readonly ILogger<QueueController> logger;
		private readonly IQueueItemRepository queueItemRepository;

		public QueueController(ILogger<QueueController> logger, IQueueItemRepository queueItemRepository)
		{
			this.logger = logger;
			this.queueItemRepository = queueItemRepository;
		}

		[HttpPost]
		[Authorize]
		public async Task<ActionResult<EnqueueResponse>> Post(EnqueueRequest request)
		{
			if(request.ExecuteOn.HasValue && request.ExecuteOn < DateTime.UtcNow)
			{
				return StatusCode(500, new ApiResponse { Success = false, ErrorMessage = "ExecuteOn must either be null for immidiate execution or a time (UTC) in the future." });
			}


			try
			{
				var queueItem = await queueItemRepository.InsertQueueItem(request.QueueName, request.Payload, request.ExecuteOn, request.UniqueKey);

				var response = new EnqueueResponse()
				{
					Success = true,
					CreateDate = queueItem.CreateDate,
					State = queueItem.State.ToString("g"),
					UniqueKey = queueItem.UniqueKey
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				var executeAfter = request.ExecuteOn.HasValue ? request.ExecuteOn.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Now";
				logger.LogError(ex, $"Failed to enqueue into worker/queue: '{request.QueueName}' with unique key: '{request.UniqueKey ?? "No unique key set"}' to be executed after: '{executeAfter}' with payload: {request.Payload}");
				return StatusCode(500, new ApiResponse { Success = false, ErrorMessage = ex.Message });
			}
		}
	}

}
