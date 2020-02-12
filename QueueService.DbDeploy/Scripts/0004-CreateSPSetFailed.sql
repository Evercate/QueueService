/****** Object:  StoredProcedure [dbo].[SetFailed]    Script Date: 1/8/2020 12:03:25 PM ******/

CREATE PROCEDURE [dbo].[SetFailed]
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
	declare @NewState tinyint

	update [QueueItem] set [State] = 
	case 
	when [Tries] > @Retries then 3
	else 0
	end
	where Id = @Id

END
GO


