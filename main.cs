using System;
using System.Collections.Generic;

public readonly record struct DirList(List<string> dirContent);
public readonly record struct DirContent(List<DirContent> dirContent);

class Program {
  public static void Main (string[] args) {
    Console.WriteLine ("Hello World");
  }
}