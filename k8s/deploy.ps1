# deploy.ps1
# Deploys the University microservices to local Kubernetes (Docker Desktop)
# Run from the LabIndividual-University folder

Write-Host "=== Step 1: Building Docker images ===" -ForegroundColor Cyan
docker-compose build
docker tag apigateway-individual:latest apigateway-individual:v2

Write-Host "`n=== Step 2: Loading images into Kind ===" -ForegroundColor Cyan
# Kind uses containerd, so local Docker images need to be explicitly loaded
kind load docker-image studentsapi-individual --name docker-desktop
kind load docker-image coursesapi-individual --name docker-desktop
kind load docker-image apigateway-individual --name docker-desktop
kind load docker-image webapp-individual --name docker-desktop

Write-Host "`n=== Step 3: Creating namespace and secrets ===" -ForegroundColor Cyan
kubectl apply -f k8s/namespace.yaml
# Same .env as docker-compose; dry-run + apply makes this re-runnable
if (-not (Test-Path .env)) { Write-Error "Missing .env - copy .env.example to .env first"; exit 1 }
kubectl create secret generic university-secrets -n university --from-env-file=.env --dry-run=client -o yaml | kubectl apply -f -

Write-Host "`n=== Step 4: Deploying infrastructure (SQL, RabbitMQ, Seq) ===" -ForegroundColor Cyan
kubectl apply -f k8s/sqldata.yaml
kubectl apply -f k8s/rabbitmq.yaml
kubectl apply -f k8s/seq.yaml

Write-Host "`n=== Step 5: Waiting for infrastructure to start (30s) ===" -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "`n=== Step 6: Deploying application services ===" -ForegroundColor Cyan
kubectl apply -f k8s/studentsapi.yaml
kubectl apply -f k8s/coursesapi.yaml
kubectl apply -f k8s/apigateway.yaml
kubectl apply -f k8s/webapp.yaml

Write-Host "`n=== Step 7: Deploying monitoring access ===" -ForegroundColor Cyan
kubectl apply -f k8s/monitoring-access.yaml

Write-Host "`n=== Step 8: Waiting for pods to start (30s) ===" -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "`n=== Deployment Status ===" -ForegroundColor Green
kubectl get pods -n university
kubectl get svc -n university

Write-Host "`n=== Access URLs ===" -ForegroundColor Green
Write-Host "WebApp:              http://localhost:30090"
Write-Host "API Gateway:         http://localhost:30080/Students"
Write-Host "StudentsAPI direct:  http://localhost:30082/api/Students"
Write-Host "CoursesAPI direct:   http://localhost:30081/api/Courses"
Write-Host "RabbitMQ Management: http://localhost:30672 (credentials from .env)"
Write-Host "Seq Logs:            http://localhost:30340"
