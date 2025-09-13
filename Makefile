.PHONY: help lint format check analyze build clean restore test

# Default target
help:
	@echo "Godot C# Project - Development Tools"
	@echo ""
	@echo "Available targets:"
	@echo "  make lint      - Run all linting and formatting checks"
	@echo "  make format    - Auto-fix all formatting issues"
	@echo "  make check     - Run checks without fixing"
	@echo "  make analyze   - Run only code analysis"
	@echo "  make build     - Build the project"
	@echo "  make clean     - Clean build artifacts"
	@echo "  make restore   - Restore NuGet packages"
	@echo "  make test      - Run tests (if available)"

# Run all checks
lint:
	@./lint.sh --check

# Auto-fix formatting issues
format:
	@./lint.sh --fix

# Check without fixing
check:
	@./lint.sh --check

# Run only analyzers
analyze:
	@./lint.sh --analyze

# Build the project
build:
	@echo "Building project..."
	@dotnet build

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@dotnet clean
	@rm -rf bin/ obj/

# Restore packages
restore:
	@echo "Restoring NuGet packages..."
	@dotnet restore

# Run tests (placeholder - add your test command here)
test:
	@echo "Running tests..."
	@# dotnet test

# Install development tools
install-tools:
	@echo "Installing development tools..."
	@dotnet tool install -g dotnet-format || true
	@echo "Tools installed successfully"

# Quick format and check before commit
pre-commit: format check
	@echo "Ready to commit!"