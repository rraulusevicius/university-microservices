# teardown.ps1
# Removes all University microservice resources from Kubernetes

Write-Host "=== Removing all resources from university namespace ===" -ForegroundColor Yellow
kubectl delete -f k8s/monitoring-access.yaml 2>$null
kubectl delete -f k8s/webapp.yaml 2>$null
kubectl delete -f k8s/apigateway.yaml 2>$null
kubectl delete -f k8s/coursesapi.yaml 2>$null
kubectl delete -f k8s/studentsapi.yaml 2>$null
kubectl delete -f k8s/seq.yaml 2>$null
kubectl delete -f k8s/rabbitmq.yaml 2>$null
kubectl delete -f k8s/sqldata.yaml 2>$null
kubectl delete -f k8s/namespace.yaml 2>$null

Write-Host "`n=== All resources removed ===" -ForegroundColor Green
