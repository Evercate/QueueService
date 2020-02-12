ALTER PROCEDURE [dbo].[SetFailed]
	-- Add the parameters for the stored procedure here
	@Id bigint,
	@ExecuteResult varchar(MAX),
	@NextRun datetime
AS
BEGIN

	update [dbo].[QueueItem] 
	set Tries = Tries + 1,
	[ExecuteTimeEnd] = GETUTCDATE(),
	[ExecuteResult] = @ExecuteResult,
	[ExecuteTimeNext] = @NextRun
	where Id = @Id

	declare @QueueWorkerId bigint

	select @QueueWorkerId = [QueueWorkerId]
	from [dbo].[QueueItem]
	where Id = @Id

	declare @Retries tinyint

	select @Retries = [Retries] from [dbo].[QueueWorker] where Id = @QueueWorkerId

	update [QueueItem] set [State] = 
	case 
	when [Tries] > @Retries then 3
	else 0
	end
	output inserted.*
	where Id = @Id

END