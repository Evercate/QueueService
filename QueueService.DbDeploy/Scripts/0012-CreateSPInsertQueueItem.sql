CREATE PROCEDURE InsertQueueItem
	@WorkerName varchar(255),
	@Payload varchar(max) = null,
	@ExecuteOn datetime = null,
	@UniqueKey varchar(255) = null
AS
BEGIN
	SET NOCOUNT ON;

	if @UniqueKey is not null
	begin
		if exists(select id from QueueItem where UniqueKey = @UniqueKey)
		begin
			select * from QueueItem where UniqueKey = @UniqueKey
			return 
		end
	end


	declare @WorkerId bigint
	Select @WorkerId = Id from QueueWorker
	where [Name] = @WorkerName

	insert [dbo].[QueueItem] ([QueueWorkerId], [Payload], [ExecuteTimeNext], [UniqueKey])
	values (@WorkerId, @Payload, @ExecuteOn, @UniqueKey)

	declare @rowId bigint
	set @rowId = SCOPE_IDENTITY()

	select * from QueueItem where Id = @rowId
    
END
GO