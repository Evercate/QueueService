docker build -f ./QueueService.Api/Dockerfile -t queue/api:latest .

docker build -f ./QueueService.Runner/Dockerfile -t queue/runner:latest .

Read-Host -Prompt "Enter to exit"