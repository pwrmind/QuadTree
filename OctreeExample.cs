public class OctreeExample
{
    public static void Run()
    {
        var octree = new OctreeAddressSystem(depth: 5);

        // Add 3D data points
        octree.AddData(0.2, 0.8, 0.9, "Drone A");    // Front-Top-Left (A)
        octree.AddData(0.7, 0.7, 0.8, "Drone B");    // Front-Top-Right (B)
        octree.AddData(0.3, 0.3, 0.9, "Sensor C");   // Front-Bottom-Left (C)
        octree.AddData(0.1, 0.1, 0.1, "Device D");   // Back-Bottom-Left (G)

        // Get by precise address
        Console.WriteLine("Data in cube 'AAA': " +
            string.Join(", ", octree.GetByAddress("AAA")));

        // Get all in front-top region
        Console.WriteLine("Front-Top region data: " +
            string.Join(", ", octree.GetByPrefix("A")));

        // Query 3D region (front half space)
        Console.WriteLine("Front half-space: " +
            string.Join(", ", octree.QueryRegion(0, 1, 0, 1, 0.5, 1)));

        // Get cube boundaries
        var bbox = octree.AddressToBBox("AAA");
        Console.WriteLine($"Cube 'AAA' boundaries: " +
            $"X: [{bbox.xmin:F2}-{bbox.xmax:F2}], " +
            $"Y: [{bbox.ymin:F2}-{bbox.ymax:F2}], " +
            $"Z: [{bbox.zmin:F2}-{bbox.zmax:F2}]");
            
        // Add to known 3D location
        octree.AddDataByAddress("ABGHE", "CT Scan Data");

        // Add through coordinates
        octree.AddData(0.35, 0.45, 0.25, "MRI Scan");

        // Batch add to same location
        string researchCube = "CDEFG";
        octree.AddDataByAddress(researchCube, "Sample 1");
        octree.AddDataByAddress(researchCube, "Sample 2");
        octree.AddDataByAddress(researchCube, "Sample 3");

        // Retrieve research data
        Console.WriteLine($"Research cube contains {octree.GetByAddress(researchCube).Count} items");
        // Output: Research cube contains 3 items
    }
}
