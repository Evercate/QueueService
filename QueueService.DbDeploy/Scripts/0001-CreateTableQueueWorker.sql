IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QueueWorker' and xtype='U')
BEGIN
CREATE TABLE [dbo].[QueueWorker](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Endpoint] [varchar](255) NOT NULL,
	[Method] [varchar](31) NOT NULL,
	[Priority] [smallint] NOT NULL,
	[Retries] [smallint] NOT NULL,
	[MaxProcessingTime] [smallint] NOT NULL,
	[BatchSize] [smallint] NOT NULL,
	[Enabled] [bit] NOT NULL,
	[RetryDelay] [smallint] NOT NULL,
	[RetryDelayMultiplier] [smallint] NOT NULL,
 CONSTRAINT [PK_QueueWorker] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_Method]  DEFAULT ('POST') FOR [Method]


ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_Priority]  DEFAULT ((0)) FOR [Priority]


ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_Retries]  DEFAULT ((0)) FOR [Retries]


ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_MaxProcessingTime]  DEFAULT ((30)) FOR [MaxProcessingTime]


ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_BatchSize]  DEFAULT ((1)) FOR [BatchSize]


ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_Enabled]  DEFAULT ((0)) FOR [Enabled]

ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_RetryDelay]  DEFAULT ((1)) FOR [RetryDelay]

ALTER TABLE [dbo].[QueueWorker] ADD  CONSTRAINT [DF_QueueWorker_RetryDelayMultiplier]  DEFAULT ((5)) FOR [RetryDelayMultiplier]

END