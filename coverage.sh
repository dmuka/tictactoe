#!/bin/bash

# Configuration
TEST_RESULTS_DIR="./TestResults"
COVERAGE_REPORT_DIR="./CoverageReport"
COLLECT_REPORTS_DIR="$TEST_RESULTS_DIR/CoverageReports"

# Clean previous results
echo "Cleaning previous test results..."
rm -rf "$TEST_RESULTS_DIR" "$COVERAGE_REPORT_DIR"
mkdir -p "$TEST_RESULTS_DIR" "$COLLECT_REPORTS_DIR"

# Run tests with code coverage using modern collection
echo "Running tests with code coverage collection..."
dotnet test \
    --collect:"XPlat Code Coverage" \
    --settings coverlet.runsettings \
    --results-directory "$TEST_RESULTS_DIR" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# Check if tests succeeded
if [ $? -ne 0 ]; then
    echo "❌ Error: Tests failed"
    exit 1
fi

# Install ReportGenerator if not exists
if ! dotnet tool list -g | grep -q "dotnet-reportgenerator-globaltool"; then
    echo "Installing ReportGenerator..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Find and copy all coverage files with unique names
echo "Preparing coverage reports..."
counter=1
find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" | while read -r file; do
    cp "$file" "$COLLECT_REPORTS_DIR/coverage_$counter.cobertura.xml"
    ((counter++))
done

# Verify we found coverage files
if [ -z "$(ls -A $COLLECT_REPORTS_DIR)" ]; then
    echo "❌ Error: No coverage files found"
    exit 1
fi

# Generate HTML report from all coverage files
echo "Generating HTML coverage report..."
reportgenerator \
    -reports:"$COLLECT_REPORTS_DIR/*.xml" \
    -targetdir:"$COVERAGE_REPORT_DIR" \
    -reporttypes:Html \
    -sourcedirs:"$PWD/src" \
    -assemblyfilters:"-*.Tests" \
    -classfilters:"-*.Tests.*" \
    -verbosity:Warning

echo "✅ Done - Coverage report available at $COVERAGE_REPORT_DIR/index.html"