# Simple Load Test for PayrollService
# Run: powershell -ExecutionPolicy Bypass -File load-test.ps1

$baseUrl = "http://localhost:5002/api"
$iterations = 20

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "PAYROLL SERVICE BASELINE LOAD TEST" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# TEST 1: Get Employees
Write-Host "TEST 1: Get Employees (READ)" -ForegroundColor Green
$times = @()
for ($i = 1; $i -le $iterations; $i++) {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/payroll/employees" -Method GET -TimeoutSec 10 -ErrorAction Stop
        $stopwatch.Stop()
        $times += $stopwatch.ElapsedMilliseconds
        Write-Host "Request $i : $($stopwatch.ElapsedMilliseconds)ms"
    }
    catch {
        Write-Host "Request $i : FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }
}

if ($times.Count -gt 0) {
    $avg = ($times | Measure-Object -Average).Average
    $min = ($times | Measure-Object -Minimum).Minimum
    $max = ($times | Measure-Object -Maximum).Maximum
    Write-Host ""
    Write-Host "Result: Avg=$([Math]::Round($avg, 2))ms | Min=$min | Max=$max" -ForegroundColor Yellow
}
Write-Host ""

# TEST 2: Calculate Payroll
Write-Host "TEST 2: Calculate Payroll (READ + COMPUTE)" -ForegroundColor Green
$times = @()
$payload = @{
    employeeId = 1
    baseSalary = 50000
    deductions = 5000
} | ConvertTo-Json

for ($i = 1; $i -le $iterations; $i++) {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/payroll/calculate" -Method POST -Body $payload -ContentType "application/json" -TimeoutSec 10 -ErrorAction Stop
        $stopwatch.Stop()
        $times += $stopwatch.ElapsedMilliseconds
        Write-Host "Request $i : $($stopwatch.ElapsedMilliseconds)ms"
    }
    catch {
        Write-Host "Request $i : FAILED" -ForegroundColor Red
    }
}

if ($times.Count -gt 0) {
    $avg = ($times | Measure-Object -Average).Average
    $min = ($times | Measure-Object -Minimum).Minimum
    $max = ($times | Measure-Object -Maximum).Maximum
    Write-Host ""
    Write-Host "Result: Avg=$([Math]::Round($avg, 2))ms | Min=$min | Max=$max" -ForegroundColor Yellow
}
Write-Host ""

# TEST 3: Process Payroll
Write-Host "TEST 3: Process Payroll (WRITE)" -ForegroundColor Green
$times = @()
$payload = @{
    employeeId = 1
    grossSalary = 50000
    netSalary = 45000
} | ConvertTo-Json

for ($i = 1; $i -le $iterations; $i++) {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/payroll/process" -Method POST -Body $payload -ContentType "application/json" -TimeoutSec 10 -ErrorAction Stop
        $stopwatch.Stop()
        $times += $stopwatch.ElapsedMilliseconds
        Write-Host "Request $i : $($stopwatch.ElapsedMilliseconds)ms"
    }
    catch {
        Write-Host "Request $i : FAILED" -ForegroundColor Red
    }
}

if ($times.Count -gt 0) {
    $avg = ($times | Measure-Object -Average).Average
    $min = ($times | Measure-Object -Minimum).Minimum
    $max = ($times | Measure-Object -Maximum).Maximum
    Write-Host ""
    Write-Host "Result: Avg=$([Math]::Round($avg, 2))ms | Min=$min | Max=$max" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "LOAD TEST COMPLETE" -ForegroundColor Cyan
Write-Host "Save these numbers! You'll compare after optimization." -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan