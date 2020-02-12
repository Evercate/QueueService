IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QueueItem' and xtype='U')
BEGIN
CREATE TABLE [dbo].[QueueItem](
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
 CONSTRAINT [PK_QueueItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


ALTER TABLE [dbo].[QueueItem] ADD  CONSTRAINT [DF_QueueItem_State]  DEFAULT ((0)) FOR [State]


ALTER TABLE [dbo].[QueueItem] ADD  CONSTRAINT [DF_QueueItem_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]


ALTER TABLE [dbo].[QueueItem] ADD  CONSTRAINT [DF_QueueItem_Tries]  DEFAULT ((0)) FOR [Tries]


ALTER TABLE [dbo].[QueueItem]  WITH CHECK ADD  CONSTRAINT [FK_QueueItem_QueueWorker] FOREIGN KEY([QueueWorkerId])
REFERENCES [dbo].[QueueWorker] ([Id])


ALTER TABLE [dbo].[QueueItem] CHECK CONSTRAINT [FK_QueueItem_QueueWorker]
END