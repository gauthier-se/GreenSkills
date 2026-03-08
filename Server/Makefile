.PHONY: help up down db-up db-down db-reset run build vet test test-v test-cover clean

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'

up: ## Start full stack (API + PostgreSQL)
	docker compose up -d --build

down: ## Stop full stack
	docker compose down

db-up: ## Start PostgreSQL only
	docker compose up -d postgres

db-down: ## Stop PostgreSQL
	docker compose down

db-reset: ## Reset database (destroy volume and restart)
	docker compose down -v
	docker compose up -d postgres

run: ## Run the server locally (requires db-up)
	go run ./cmd/server

build: ## Build the server binary
	go build -o bin/server ./cmd/server

vet: ## Run go vet static analysis
	go vet ./...

test: ## Run all tests
	go test ./...

test-v: ## Run all tests with verbose output
	go test -v ./...

test-cover: ## Run tests with coverage report
	go test -v -cover ./...

clean: ## Remove build artifacts
	rm -rf bin/
