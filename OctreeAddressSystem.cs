public class OctreeAddressSystem
{
    private readonly int depth;
    private readonly Dictionary<string, List<object>> storage;

    public OctreeAddressSystem(int depth = 5)
    {
        this.depth = depth;
        this.storage = new Dictionary<string, List<object>>();
    }

    public string PointToAddress(double x, double y, double z)
    {
        double xmin = 0.0, ymin = 0.0, zmin = 0.0;
        double xmax = 1.0, ymax = 1.0, zmax = 1.0;
        char[] address = new char[depth];

        for (int i = 0; i < depth; i++)
        {
            double midX = (xmin + xmax) / 2;
            double midY = (ymin + ymax) / 2;
            double midZ = (zmin + zmax) / 2;

            // Clamp coordinates to valid range
            x = Math.Clamp(x, xmin, xmax - 1e-10);
            y = Math.Clamp(y, ymin, ymax - 1e-10);
            z = Math.Clamp(z, zmin, zmax - 1e-10);

            // Determine octant
            if (x < midX && y >= midY && z >= midZ)      // Front-Top-Left
                address[i] = 'A';
            else if (x >= midX && y >= midY && z >= midZ) // Front-Top-Right
                address[i] = 'B';
            else if (x < midX && y < midY && z >= midZ)   // Front-Bottom-Left
                address[i] = 'C';
            else if (x >= midX && y < midY && z >= midZ)  // Front-Bottom-Right
                address[i] = 'D';
            else if (x < midX && y >= midY && z < midZ)   // Back-Top-Left
                address[i] = 'E';
            else if (x >= midX && y >= midY && z < midZ)  // Back-Top-Right
                address[i] = 'F';
            else if (x < midX && y < midY && z < midZ)    // Back-Bottom-Left
                address[i] = 'G';
            else                                          // Back-Bottom-Right
                address[i] = 'H';

            // Update boundaries based on octant
            switch (address[i])
            {
                case 'A':
                    xmax = midX; ymin = midY; zmin = midZ;
                    break;
                case 'B':
                    xmin = midX; ymin = midY; zmin = midZ;
                    break;
                case 'C':
                    xmax = midX; ymax = midY; zmin = midZ;
                    break;
                case 'D':
                    xmin = midX; ymax = midY; zmin = midZ;
                    break;
                case 'E':
                    xmax = midX; ymin = midY; zmax = midZ;
                    break;
                case 'F':
                    xmin = midX; ymin = midY; zmax = midZ;
                    break;
                case 'G':
                    xmax = midX; ymax = midY; zmax = midZ;
                    break;
                case 'H':
                    xmin = midX; ymax = midY; zmax = midZ;
                    break;
            }
        }

        return new string(address);
    }

    public (double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        AddressToBBox(string address)
    {
        double xmin = 0.0, ymin = 0.0, zmin = 0.0;
        double xmax = 1.0, ymax = 1.0, zmax = 1.0;

        foreach (char c in address)
        {
            double midX = (xmin + xmax) / 2;
            double midY = (ymin + ymax) / 2;
            double midZ = (zmin + zmax) / 2;

            switch (c)
            {
                case 'A': // Front-Top-Left
                    xmax = midX; ymin = midY; zmin = midZ;
                    break;
                case 'B': // Front-Top-Right
                    xmin = midX; ymin = midY; zmin = midZ;
                    break;
                case 'C': // Front-Bottom-Left
                    xmax = midX; ymax = midY; zmin = midZ;
                    break;
                case 'D': // Front-Bottom-Right
                    xmin = midX; ymax = midY; zmin = midZ;
                    break;
                case 'E': // Back-Top-Left
                    xmax = midX; ymin = midY; zmax = midZ;
                    break;
                case 'F': // Back-Top-Right
                    xmin = midX; ymin = midY; zmax = midZ;
                    break;
                case 'G': // Back-Bottom-Left
                    xmax = midX; ymax = midY; zmax = midZ;
                    break;
                case 'H': // Back-Bottom-Right
                    xmin = midX; ymax = midY; zmax = midZ;
                    break;
                default:
                    throw new ArgumentException($"Invalid character in address: '{c}'");
            }
        }

        return (xmin, xmax, ymin, ymax, zmin, zmax);
    }

    public void AddData(double x, double y, double z, object data)
    {
        string address = PointToAddress(x, y, z);
        if (!storage.ContainsKey(address))
        {
            storage[address] = new List<object>();
        }
        storage[address].Add(data);
    }

    public List<object> GetByAddress(string address)
    {
        return storage.TryGetValue(address, out var data) ? data : new List<object>();
    }

    public List<object> GetByPrefix(string prefix)
    {
        return storage
            .Where(kvp => kvp.Key.StartsWith(prefix))
            .SelectMany(kvp => kvp.Value)
            .ToList();
    }

    public List<object> QueryRegion(
        double qxmin, double qxmax,
        double qymin, double qymax,
        double qzmin, double qzmax)
    {
        var results = new List<object>();

        foreach (var kvp in storage)
        {
            var (axmin, axmax, aymin, aymax, azmin, azmax) = AddressToBBox(kvp.Key);

            // Check for 3D overlap
            bool xOverlap = axmin < qxmax && axmax > qxmin;
            bool yOverlap = aymin < qymax && aymax > qymin;
            bool zOverlap = azmin < qzmax && azmax > qzmin;

            if (xOverlap && yOverlap && zOverlap)
            {
                results.AddRange(kvp.Value);
            }
        }

        return results;
    }
    
    /// Add data directly to a specific 3D address
    /// </summary>
    /// <param name="address">Target octree address</param>
    /// <param name="data">Data to store</param>
    public void AddDataByAddress(string address, object data)
    {
        ValidateAddress(address);
        
        if (!storage.ContainsKey(address))
        {
            storage[address] = new List<object>();
        }
        storage[address].Add(data);
    }

    /// <summary>
    /// Validate 3D address format
    /// </summary>
    private void ValidateAddress(string address)
    {
        if (address.Length != depth)
        {
            throw new ArgumentException($"Address length must be {depth}. Got {address.Length}");
        }

        foreach (char c in address)
        {
            if (c < 'A' || c > 'H')
            {
                throw new ArgumentException($"Invalid character '{c}' in address. Only A-H allowed");
            }
        }
    }
}
