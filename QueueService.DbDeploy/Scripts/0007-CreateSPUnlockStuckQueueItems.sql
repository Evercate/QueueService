CREATE PROCEDURE UnlockStuckQueueItems
AS
BEGIN
	SET NOCOUNT ON;

	declare @now datetime
	set @now = GETUTCDATE()

	update QueueItem
	set State = 0, Tries = 0
	from QueueItem qi 
	inner join [dbo].[QueueWorker] qw on qi.QueueWorkerId = qw.Id
	where qi.State = 1 and DATEADD(SECOND, qw.MaxProcessingTime, qi.ExecuteTimeStart) < @now
END