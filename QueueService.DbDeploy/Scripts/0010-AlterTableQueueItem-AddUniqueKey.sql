ALTER TABLE dbo.QueueItem ADD
[UniqueKey] varchar(255) NULL


ALTER TABLE dbo.QueueItemArchive ADD
[UniqueKey] varchar(255) NULL

go

CREATE UNIQUE INDEX UX_QueueItem_UniqueKey
ON QueueItem ([UniqueKey])
WHERE [UniqueKey] IS NOT NULL