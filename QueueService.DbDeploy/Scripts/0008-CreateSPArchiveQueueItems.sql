﻿CREATE PROCEDURE ArchiveQueueItems
	@days int = 30
AS
BEGIN
	SET NOCOUNT ON;

	declare @now datetime
	set @now = GETUTCDATE()

    delete [dbo].[QueueItem]
	output 
	deleted.[QueueWorkerId], 
	deleted.[State], 
	deleted.[CreateDate], 
	deleted.[Payload], 
	deleted.[Tries], 
	deleted.[ExecuteTimeStart], 
	deleted.[ExecuteTimeEnd], 
	deleted.[ExecuteTimeNext], 
	deleted.[ExecuteResult] 
	into [dbo].[QueueItemArchive](
	[QueueWorkerId], 
	[State], 
	[CreateDate], 
	[Payload], 
	[Tries], 
	[ExecuteTimeStart], 
	[ExecuteTimeEnd], 
	[ExecuteTimeNext], 
	[ExecuteResult]
	)
	where [State] = 2 and DATEADD(day, @days, CreateDate) < @now
	
	
END