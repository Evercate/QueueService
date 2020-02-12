CREATE PROCEDURE [dbo].[GetNextId]
	-- Add the parameters for the stored procedure here
	@WorkerId bigint
AS
BEGIN
	SET NOCOUNT ON;


	DECLARE @MaxRetries tinyint
	declare @BatchSize tinyint
	select 
	@MaxRetries = Retries,
	@BatchSize = [BatchSize]
	from [dbo].[QueueWorker]
	where Id = @WorkerId

	DECLARE @Now datetime
	set @Now = GETUTCDATE()

	declare @Ids table (Id bigint NOT NULL PRIMARY KEY CLUSTERED)

	BEGIN TRANSACTION

	insert into @Ids(Id)
	SELECT TOP(@BatchSize) Id
	FROM QueueItem WITH (UPDLOCK, READPAST)
	WHERE [State] = 0 
	and QueueWorkerId = @WorkerId
	and Tries <= @MaxRetries
	and ([ExecuteTimeNext] is null or [ExecuteTimeNext] < @Now)
	ORDER BY ID ASC


	UPDATE QueueItem SET 
	[State] = 1,
	[ExecuteTimeStart] = @Now
	WHERE ID in (select Id from @Ids)

	COMMIT TRANSACTION


	SELECT * FROM QueueItem WITH (NOLOCK)
	WHERE ID in (select Id from @Ids)
END
GO