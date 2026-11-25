namespace Backend.Services;

public class TelemetryArrayService
{
    public int[][] CreateTelemetryGroups(int totalReadings)
    {
        if (totalReadings < 1)
        {
            return [];
        }

        int firstGroupSize = Math.Min(5, totalReadings);
        int remaining = totalReadings - firstGroupSize;

        int[][] groups = remaining > 0
            ? new int[2][]
            : new int[1][];

        groups[0] = new int[firstGroupSize];

        for (int i = 0; i < firstGroupSize; i++)
        {
            groups[0][i] = i + 1;
        }

        if (remaining > 0)
        {
            groups[1] = new int[remaining];

            for (int i = 0; i < remaining; i++)
            {
                groups[1][i] = firstGroupSize + i + 1;
            }
        }

        return groups;
    }

    public int CountValues(int[][] groups)
    {
        int total = 0;

        foreach (int[] group in groups)
        {
            total += group.Length;
        }

        return total;
    }
}