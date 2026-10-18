# portforward.ps1 - Opens all port forwards for Kubernetes services
Start-Process powershell -ArgumentList "-Command kubectl port-forward svc/webapp 30090:8080 -n university"
Start-Process powershell -ArgumentList "-Command kubectl port-forward svc/apigateway 30080:8080 -n university"
Start-Process powershell -ArgumentList "-Command kubectl port-forward svc/seq 30340:80 -n university"
Start-Process powershell -ArgumentList "-Command kubectl port-forward svc/rabbitmq 30672:15672 -n university"
Start-Process powershell -ArgumentList "-Command kubectl port-forward svc/studentsapi-external 30082:8080 -n university"
Start-Process powershell -ArgumentList "-Command kubectl port-forward svc/coursesapi-external 30081:8080 -n university"
Write-Host "All port-forwards started!" -ForegroundColor Green
Write-Host "WebApp:      http://localhost:30090"
Write-Host "Gateway:     http://localhost:30080/Students"
Write-Host "StudentsAPI: http://localhost:30082/swagger"
Write-Host "CoursesAPI:  http://localhost:30081/swagger"
Write-Host "Seq:         http://localhost:30340"
Write-Host "RabbitMQ:    http://localhost:30672"