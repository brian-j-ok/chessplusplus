.PHONY: help lint format check analyze build clean restore test install-tools format-check pre-commit

# Default target
help:
	@echo "Godot C# Project - Development Tools"
	@echo ""
	@echo "Available targets:"
	@echo "  make lint         - Run all linting and analysis checks"
	@echo "  make format       - Auto-fix all formatting issues"
	@echo "  make format-check - Check formatting without fixing"
	@echo "  make check        - Run all checks (lint + format-check)"
	@echo "  make analyze      - Run code analysis only"
	@echo "  make build        - Build the project"
	@echo "  make clean        - Clean build artifacts"
	@echo "  make restore      - Restore NuGet packages"
	@echo "  make test         - Run tests (if available)"
	@echo "  make install-tools- Install global .NET tools"
	@echo "  make pre-commit   - Run format and checks before commit"

# Run code analysis (StyleCop, Roslynator, and built-in analyzers)
analyze:
	@echo "Running code analysis..."
	@dotnet build -warnaserror

# Run linting (analysis without fixing)
lint:
	@echo "Running linters..."
	@dotnet build --no-incremental

# Auto-fix formatting issues with CSharpier only (dotnet format has issues with Godot)
format:
	@echo "Formatting code with CSharpier..."
	@~/.dotnet/tools/dotnet-csharpier .
	@echo "✅ Code formatted successfully"

# Check formatting without fixing
format-check:
	@echo "Checking code format with CSharpier..."
	@~/.dotnet/tools/dotnet-csharpier --check .

# Run all checks
check: format-check lint

# Build the project
build:
	@echo "Building project..."
	@dotnet build

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@dotnet clean
	@rm -rf bin/ obj/
	@rm -rf .godot/mono/temp/bin/
	@rm -rf .godot/mono/temp/obj/

# Restore packages
restore:
	@echo "Restoring NuGet packages..."
	@dotnet restore

# Run tests
test:
	@echo "Running tests..."
	@# dotnet test

# Install global development tools
install-tools:
	@echo "Installing global .NET tools..."
	@dotnet tool install -g csharpier || dotnet tool update -g csharpier
	@dotnet tool install -g dotnet-format || dotnet tool update -g dotnet-format
	@echo "Tools installed/updated successfully"
	@echo ""
	@echo "Installed tools:"
	@dotnet tool list -g | grep -E "(csharpier|dotnet-format)" || true

# Quick format and check before commit
pre-commit:
	@echo "Running pre-commit checks..."
	@echo "1. Formatting code..."
	@~/.dotnet/tools/dotnet-csharpier .
	@echo "2. Verifying format..."
	@~/.dotnet/tools/dotnet-csharpier --check .
	@echo "3. Building project to check for errors..."
	@dotnet build --no-incremental
	@echo "✅ All pre-commit checks passed - ready to commit!"

# Watch mode for development
watch:
	@echo "Starting file watcher..."
	@while true; do \
		inotifywait -q -r -e modify,create,delete --exclude '\.godot|bin|obj|\.git' *.cs 2>/dev/null || fswatch -r --exclude '\.godot' --exclude 'bin' --exclude 'obj' --exclude '\.git' -1 .; \
		clear; \
		echo "Files changed, running checks..."; \
		$(MAKE) format-check; \
		echo ""; \
		echo "Watching for changes... (Press Ctrl+C to stop)"; \
	done

# CI-specific targets
ci-check:
	@echo "Running CI checks..."
	@dotnet restore
	@dotnet build --no-restore
	@dotnet csharpier --check .
	@dotnet format style --verify-no-changes --no-restore --verbosity diagnostic
	@dotnet format analyzers --verify-no-changes --no-restore --verbosity diagnostic