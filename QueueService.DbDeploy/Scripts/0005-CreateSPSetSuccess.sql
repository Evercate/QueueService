/****** Object:  StoredProcedure [dbo].[SetSuccess]    Script Date: 1/8/2020 12:04:26 PM ******/

CREATE PROCEDURE [dbo].[SetSuccess]
	-- Add the parameters for the stored procedure here
	@Id bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	update [dbo].[QueueItem] set 
	State = 2,
	Tries = Tries + 1,
	[ExecuteTimeEnd] = GETUTCDATE()
	where Id = @Id
END
GO


