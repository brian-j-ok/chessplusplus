#!/bin/bash

# Setup script for pre-commit hooks

echo "Setting up pre-commit hooks for Godot C# project..."

# Check if Python is installed
if ! command -v python3 &> /dev/null && ! command -v python &> /dev/null; then
    echo "❌ Python is not installed. Please install Python 3 to use pre-commit hooks."
    exit 1
fi

# Install pre-commit if not already installed
if ! command -v pre-commit &> /dev/null; then
    echo "Installing pre-commit..."
    pip install --user pre-commit || pip3 install --user pre-commit
fi

# Install the pre-commit hooks
echo "Installing pre-commit hooks..."
pre-commit install

# Install .NET tools if not already installed
echo "Checking .NET tools..."
if ! command -v dotnet-format &> /dev/null; then
    echo "Installing dotnet-format..."
    dotnet tool install -g dotnet-format
fi

if ! command -v dotnet-csharpier &> /dev/null; then
    echo "Installing CSharpier..."
    dotnet tool install -g csharpier
fi

# Run pre-commit on all files to check setup
echo ""
echo "Running initial check on all files..."
pre-commit run --all-files || true

echo ""
echo "✅ Pre-commit hooks have been set up successfully!"
echo ""
echo "The following hooks will run automatically before each commit:"
echo "  - Trailing whitespace removal"
echo "  - End of file fixer"
echo "  - YAML/JSON/XML validation"
echo "  - CSharpier formatting"
echo "  - dotnet format style checks"
echo "  - Build verification"
echo "  - Security vulnerability scan"
echo ""
echo "You can also run hooks manually with:"
echo "  pre-commit run --all-files"