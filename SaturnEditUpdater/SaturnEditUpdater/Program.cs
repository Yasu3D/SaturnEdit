using System.Diagnostics;

internal class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // args[0] = SaturnEdit.exe
            // args[1] = Download.zip
            // args[2] = Extracted
            
            string processPath = args[0];
            string downloadPath = args[1];
            string extractedDirectory = args[2];

            string processDirectory = Path.GetDirectoryName(processPath) ?? "";
            
            // Replace all files.
            DirectoryInfo source = new(extractedDirectory);
            DirectoryInfo destination = new(processDirectory);
            CopyFiles(source, destination);
            
            // Delete temporary files.
            File.Delete(downloadPath);
            DirectoryInfo extractedDirectoryInfo = new(extractedDirectory);
            extractedDirectoryInfo.Parent?.Delete(true);
            
            // Start new process.
            ProcessStartInfo process = new()
            {
                FileName = processPath,
                UseShellExecute = true,
            };

            Process.Start(process);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.ReadKey();
        }
    }
    
    private static void CopyFiles(DirectoryInfo source, DirectoryInfo destination)
    {
        Directory.CreateDirectory(destination.FullName);

        foreach (FileInfo file in source.GetFiles())
        {
            try
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
            }
            catch (Exception ex)
            {
                // IOExceptions are caught silently?
                if (ex is not IOException)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        foreach (DirectoryInfo directory in source.GetDirectories())
        {
            DirectoryInfo newDestination = destination.CreateSubdirectory(directory.Name);
            CopyFiles(directory, newDestination);
        }
    }
}