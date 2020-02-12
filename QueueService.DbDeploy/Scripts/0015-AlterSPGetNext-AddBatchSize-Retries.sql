ALTER PROCEDURE [dbo].[GetNextId]
	-- Add the parameters for the stored procedure here
	@WorkerId bigint,
	@MaxRetries tinyint = 1,
	@BatchSize tinyint = 1
AS
BEGIN
	SET NOCOUNT ON;

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
	--from QueueItem qi inner join @Ids ids on Ids.Id = qi.Id
	WHERE ID in (select Id from @Ids)

	COMMIT TRANSACTION


	SELECT * FROM QueueItem WITH (NOLOCK)
	WHERE ID in (select Id from @Ids)
END