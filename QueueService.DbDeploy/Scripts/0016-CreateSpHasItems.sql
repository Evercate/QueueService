CREATE PROCEDURE HasItems
	-- Add the parameters for the stored procedure here
	@HasItems bit output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Now datetime
	set @Now = GETUTCDATE()

	if exists(
	select qi.id  
	FROM QueueItem qi WITH(NOLOCK)
	inner join QueueWorker qw on qw.Id = qi.QueueWorkerId
	WHERE [State] = 0 
	and qi.Tries <= qw.Retries
	and ([ExecuteTimeNext] is null or [ExecuteTimeNext] < @Now)
	)
	set @HasItems = 1
	else
	set @HasItems = 0
END
GO