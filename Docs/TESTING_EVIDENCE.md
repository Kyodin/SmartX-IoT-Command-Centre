
# Smart-X Part 2 - Testing Evidence

## Introduction

For Part 2 of my Smart-X POE, I added automated testing to check that the main data structures and backend services work as expected.

I used xUnit with .NET 10 to create and execute the tests.

The tests focus on search intelligence, device operations, alert management and telemetry queue processing.

## Automated Test Results

The most recent verified test execution returned:

- Total tests: 28
- Passed: 28
- Failed: 0
- Skipped: 0

The tests were executed using:

    dotnet test Tests/SmartX.Tests.csproj

## Search Intelligence Testing

Eight tests were created to verify:

- Sensor search recording
- Search frequency counts
- Search history limits
- Recommendation thresholds
- Offline sensor escalation
- Duplicate recommendation prevention
- Independent sensor search counts

## Operations Testing

Ten tests were created to verify:

- Dictionary-based device lookup
- Invalid command handling
- Device disconnection and sensor status updates
- Stack-based undo functionality
- HashSet duplicate alert prevention
- Priority-based alert processing
- Equal-priority alert ordering
- Reusing alert types after processing
- Chronological event history
- Historical recommendation duplication prevention

## FIFO Telemetry Queue Testing

Ten tests were created to verify:

- Empty queue behaviour
- Adding telemetry readings
- FIFO processing order
- Queue count updates
- Processing an empty queue
- Viewing pending readings without removing them
- Batch insertion order
- Empty batch handling
- Multiple readings from the same sensor
- Independent pending-reading list snapshots

## Testing Evidence

The automated test results are shown in:

`Screen_Shots/PART2_AUTOMATED_TESTS.JPG`

## Testing Resources

The lecturer-provided notes, slides, practical examples and assessment materials were used to support my understanding of testing and data structure behaviour.

Additional references are provided in the main README.md.

## References

Microsoft Learn. Unit testing C# with xUnit.
https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit

xUnit.net. Official Documentation.
https://xunit.net/

Microsoft Learn. C# Collections.
https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/collections
