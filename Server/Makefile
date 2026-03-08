.PHONY: help db-up db-down db-reset run build vet test test-v test-cover clean

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'

db-up: ## Start PostgreSQL container
	docker compose up -d

db-down: ## Stop PostgreSQL container
	docker compose down

db-reset: ## Reset database (destroy volume and restart)
	docker compose down -v
	docker compose up -d

run: ## Run the server
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
