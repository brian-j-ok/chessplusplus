#!/bin/bash

# Convenience script for running linting and formatting checks

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Add .NET tools to PATH
export PATH="$PATH:/home/briano/.dotnet/tools"

# Parse command line arguments
FIX_MODE=false
CHECK_ONLY=false
FORMAT_ONLY=false
ANALYZE_ONLY=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --fix)
            FIX_MODE=true
            shift
            ;;
        --check)
            CHECK_ONLY=true
            shift
            ;;
        --format)
            FORMAT_ONLY=true
            shift
            ;;
        --analyze)
            ANALYZE_ONLY=true
            shift
            ;;
        -h|--help)
            echo "Usage: ./lint.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --fix      Fix all formatting and style issues automatically"
            echo "  --check    Run checks only, don't fix anything"
            echo "  --format   Run only formatting checks/fixes"
            echo "  --analyze  Run only code analysis"
            echo "  -h, --help Show this help message"
            echo ""
            echo "Examples:"
            echo "  ./lint.sh          # Run all checks"
            echo "  ./lint.sh --fix    # Fix all issues automatically"
            echo "  ./lint.sh --format # Check/fix only formatting"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

echo -e "${BLUE}═══════════════════════════════════════════════════${NC}"
echo -e "${BLUE}     C# Linting and Formatting Tool${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════${NC}"
echo ""

# Function to run formatting checks/fixes
run_formatting() {
    # Run dotnet-format
    if command -v dotnet-format >/dev/null 2>&1; then
        echo -e "${YELLOW}Running dotnet-format...${NC}"
        if [ "$FIX_MODE" = true ]; then
            if dotnet-format "Chess++.sln"; then
                echo -e "${GREEN}✓ Code formatted with dotnet-format${NC}"
            else
                echo -e "${RED}✗ dotnet-format failed${NC}"
                return 1
            fi
        else
            if dotnet-format "Chess++.sln" --check; then
                echo -e "${GREEN}✓ dotnet-format check passed${NC}"
            else
                echo -e "${RED}✗ Code formatting issues found${NC}"
                echo -e "${YELLOW}  Run './lint.sh --fix' to fix automatically${NC}"
                return 1
            fi
        fi
    else
        echo -e "${YELLOW}⚠ dotnet-format not installed${NC}"
        echo -e "  Install with: dotnet tool install -g dotnet-format"
    fi

}

# Function to run code analysis
run_analysis() {
    echo -e "${YELLOW}Running code analysis...${NC}"

    # First ensure packages are restored
    dotnet restore > /dev/null 2>&1

    # Run build with analyzers
    ANALYZER_OUTPUT=$(dotnet build --no-restore /p:TreatWarningsAsErrors=false 2>&1)
    BUILD_EXIT_CODE=$?

    if [ $BUILD_EXIT_CODE -ne 0 ]; then
        echo -e "${RED}✗ Build failed${NC}"
        echo "$ANALYZER_OUTPUT" | grep -E "error CS"
        return 1
    fi

    # Check for warnings
    if echo "$ANALYZER_OUTPUT" | grep -q "warning"; then
        echo -e "${YELLOW}⚠ Analyzer warnings found:${NC}"
        echo "$ANALYZER_OUTPUT" | grep "warning" | head -20
        echo ""
        echo -e "${YELLOW}Consider fixing these warnings${NC}"
    else
        echo -e "${GREEN}✓ No analyzer issues found${NC}"
    fi
}

# Main execution
FAILED=0

if [ "$FORMAT_ONLY" = true ] || [ "$ANALYZE_ONLY" = false ]; then
    echo -e "${BLUE}── Formatting Checks ──${NC}"
    run_formatting || FAILED=1
    echo ""
fi

if [ "$ANALYZE_ONLY" = true ] || [ "$FORMAT_ONLY" = false ]; then
    echo -e "${BLUE}── Code Analysis ──${NC}"
    run_analysis || FAILED=1
    echo ""
fi

# Summary
echo -e "${BLUE}═══════════════════════════════════════════════════${NC}"
if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ All checks passed!${NC}"
else
    echo -e "${RED}✗ Some checks failed${NC}"
    if [ "$FIX_MODE" = false ] && [ "$CHECK_ONLY" = false ]; then
        echo -e "${YELLOW}Run './lint.sh --fix' to automatically fix formatting issues${NC}"
    fi
fi
echo -e "${BLUE}═══════════════════════════════════════════════════${NC}"

exit $FAILED