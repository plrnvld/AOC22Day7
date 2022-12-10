using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public readonly record struct FileLeaf(string Name, long Size);

public class DirNode
{
    public string Name { get; }
    public DirNode Parent { get; }
    public List<DirNode> SubDirs { get; } = new List<DirNode>();
    public List<FileLeaf> Files { get; } = new List<FileLeaf>();
    
    public DirNode(string name, DirNode parent) => (Name, Parent) = (name, parent);
    
    public long SumSize => SubDirs.Sum(d => d.SumSize) + Files.Sum(f => f.Size);

    public IEnumerable<DirNode> FlattenDirs()
    {
        var dirs = new List<DirNode>{ this };

        foreach (var subDir in SubDirs)
            dirs.AddRange(subDir.FlattenDirs());
        
        return dirs;
    }
}

class Program 
{   
    public static void Main (string[] args) 
    {
        var root = ReadFilesystem("Input.txt");

        var used = root.SumSize;
        var needed = 30_000_000L;
        var free = 70_000_000L - used;
        var toClear = needed - free;

        var sizesForDeletion = root.FlattenDirs().Select(d => d.SumSize).Where(s => s >= toClear).OrderBy(x => x);

        Console.WriteLine($"Used: {used}, clear: {toClear}");

        Console.WriteLine($"Lowest {sizesForDeletion.First()}");
    }

    static DirNode ReadFilesystem(string fileName)
    {
        var lines = File.ReadLines(fileName).ToArray();

        var root = new DirNode("/", parent: null);
        var curr = root;

        for (var i = 1 /* skip root*/; i < lines.Length; i++)
        {
            Console.WriteLine($"> Line[{i}] Current dir is '{curr.Name}'");
            var line = lines[i];
            if (line == "$ ls")
            {
                var dirContents = lines[(i+1)..].TakeWhile(l => !l.StartsWith('$'));
                var filesTexts = dirContents.Where(c => !c.StartsWith("dir"));
                var files = filesTexts.Select(t => 
                { 
                    var parts = t.Split(" "); 
                    return new FileLeaf(parts[1], long.Parse(parts[0])); 
                });
                
                curr.Files.AddRange(files.Where(f => !curr.Files.Any(fil => fil.Name == f.Name)));

                Console.WriteLine($"  Adding {dirContents.Count()} to i");
                i += dirContents.Count();
            }
            else if (line == "$ cd ..") 
            {   
                Console.WriteLine($"Parent of '{curr?.Name}' is '{curr?.Parent?.Name}''");
                curr = curr?.Parent;
            }
            else if (line.StartsWith("$ cd "))
            {
                var dirName = line.Substring(5);
                var newDir = new DirNode(dirName, curr);
                Console.WriteLine($"Found subdir '{dirName}' of dir '{curr.Name}'");
                if (!curr.SubDirs.Any(s => s.Name == newDir.Name))
                    curr.SubDirs.Add(newDir);
                curr = newDir;
            }
            else 
                throw new Exception($"Exceptional line: '{line}''");
        }

        return root;
    }
}