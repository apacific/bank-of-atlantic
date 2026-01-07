#!/bin/bash
# ============================================================================
# Banking POC - E2E Testing Helper Script (Bash version)
# ============================================================================

set -e

PROJECT_ROOT="/c/projects/software/fullstack/banking-poc"
BACKEND_DIR="$PROJECT_ROOT/backend"
FRONTEND_DIR="$PROJECT_ROOT/frontend"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_header() {
    echo -e "\n${YELLOW}========================================${NC}"
    echo -e "${YELLOW}$1${NC}"
    echo -e "${YELLOW}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

check_database() {
    docker ps 2>/dev/null | grep -q banking-db
    return $?
}

check_api() {
    curl -s http://localhost:5185/health >/dev/null 2>&1
    return $?
}

check_frontend() {
    curl -s http://localhost:5173/ >/dev/null 2>&1
    return $?
}

start_database() {
    print_header "Starting PostgreSQL Database"
    
    if docker ps 2>/dev/null | grep -q banking-db; then
        print_error "Database container already running"
        return 1
    fi
    
    docker run -d --name banking-db \
        -e POSTGRES_USER=banking \
        -e POSTGRES_PASSWORD=banking \
        -e POSTGRES_DB=banking \
        -p 5433:5432 \
        postgres:16-alpine
    
    echo "Waiting for database to initialize..."
    sleep 5
    
    if docker ps 2>/dev/null | grep -q banking-db; then
        print_success "PostgreSQL is running on port 5433"
    else
        print_error "Failed to start PostgreSQL"
        return 1
    fi
}

start_api() {
    print_header "Starting Backend API"
    cd "$BACKEND_DIR"
    dotnet run --project src/Banking.Api
}

start_frontend() {
    print_header "Starting Frontend"
    cd "$FRONTEND_DIR"
    npm run dev
}

run_e2e_tests() {
    print_header "Running E2E Tests"
    
    echo "Checking prerequisites..."
    
    if ! check_database; then
        print_error "PostgreSQL database is NOT running"
        echo "Run: start_database"
        return 1
    fi
    print_success "PostgreSQL database is running"
    
    sleep 1
    
    if ! check_api; then
        print_error "Backend API is NOT responding"
        echo "Run: start_api"
        return 1
    fi
    print_success "Backend API is running"
    
    if ! check_frontend; then
        print_error "Frontend is NOT responding"
        echo "Run: start_frontend"
        return 1
    fi
    print_success "Frontend is running"
    
    echo ""
    echo "All services verified. Running E2E tests..."
    echo ""
    
    cd "$BACKEND_DIR"
    dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj -v detailed
}

check_status() {
    print_header "Service Status"
    
    if docker ps 2>/dev/null | grep -q banking-db; then
        print_success "PostgreSQL Database - RUNNING (Port 5433)"
    else
        print_error "PostgreSQL Database - NOT RUNNING"
    fi
    
    if check_api; then
        print_success "Backend API - RUNNING (Port 5185)"
    else
        print_error "Backend API - NOT RUNNING (Port 5185)"
    fi
    
    if check_frontend; then
        print_success "Frontend - RUNNING (Port 5173)"
    else
        print_error "Frontend - NOT RUNNING (Port 5173)"
    fi
    
    echo ""
    echo "Docker containers:"
    docker ps -a 2>/dev/null | grep -i banking || echo "No banking containers found"
}

stop_services() {
    print_header "Stopping Services"
    
    echo "Stopping Docker container..."
    docker stop banking-db 2>/dev/null || true
    docker rm banking-db 2>/dev/null || true
    print_success "Docker container stopped"
    
    echo ""
    echo "Note: Use Ctrl+C in other terminals to stop:"
    echo "  - Backend API"
    echo "  - Frontend"
}

build_solution() {
    print_header "Building Solution"
    cd "$BACKEND_DIR"
    dotnet build
}

run_all_tests() {
    print_header "Running All Tests"
    
    echo "1. Running Backend Tests..."
    cd "$BACKEND_DIR"
    dotnet test
    
    echo ""
    echo "2. Running Frontend Tests..."
    cd "$FRONTEND_DIR"
    npm run test:unit -- --run
    
    echo ""
    echo "3. Running E2E Tests..."
    cd "$BACKEND_DIR"
    dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj
    
    print_success "All tests completed!"
}

# Quick commands - can be called directly
if [ $# -eq 0 ]; then
    print_header "Banking POC - E2E Testing Helper"
    echo "Usage: $0 <command>"
    echo ""
    echo "Available commands:"
    echo "  start_database    - Start PostgreSQL database"
    echo "  start_api         - Start Backend API"
    echo "  start_frontend    - Start Frontend"
    echo "  run_e2e_tests     - Run E2E tests (requires all services)"
    echo "  check_status      - Check service status"
    echo "  stop_services     - Stop all services"
    echo "  build_solution    - Build .NET solution"
    echo "  run_all_tests     - Run all tests (backend + frontend + e2e)"
    echo ""
    echo "Quick setup:"
    echo "  $0 start_database"
    echo "  # In another terminal:"
    echo "  $0 start_api"
    echo "  # In another terminal:"
    echo "  $0 start_frontend"
    echo "  # In another terminal:"
    echo "  $0 run_e2e_tests"
    exit 0
fi

case "$1" in
    start_database)
        start_database
        ;;
    start_api)
        start_api
        ;;
    start_frontend)
        start_frontend
        ;;
    run_e2e_tests)
        run_e2e_tests
        ;;
    check_status)
        check_status
        ;;
    stop_services)
        stop_services
        ;;
    build_solution)
        build_solution
        ;;
    run_all_tests)
        run_all_tests
        ;;
    *)
        echo "Unknown command: $1"
        echo "Use: $0 (no args) for help"
        exit 1
        ;;
esac
