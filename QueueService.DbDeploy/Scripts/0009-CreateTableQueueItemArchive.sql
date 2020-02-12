IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QueueItemArchive' and xtype='U')
BEGIN
CREATE TABLE [dbo].[QueueItemArchive](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[QueueWorkerId] [bigint] NOT NULL,
	[State] [smallint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Payload] [varchar](max) NULL,
	[Tries] [smallint] NOT NULL,
	[ExecuteTimeStart] [datetime] NULL,
	[ExecuteTimeEnd] [datetime] NULL,
	[ExecuteTimeNext] [datetime] NULL,
	[ExecuteResult] [varchar](max) NULL,
 CONSTRAINT [PK_QueueItemArchive] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END