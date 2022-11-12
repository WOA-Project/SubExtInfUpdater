

/*
 * Program to assist modifying subext infs for Qualcomm Windows platforms
 */
using System.Security.Cryptography;
using System.Text;

/*
 * Parameters
 */

// Can also be FASTRPC, see stock files for reference of what to use here
string protocol = "TFTP";

// The remote path to upload to the SoC
string remotePathDestinationFolder = @"\rfs\msm\mpss\readonly\firmware\image\modem_pr";

// The root directory containing the files to upload to the soc
string directory = @"C:\Users\gus33\Downloads\ota_b1-11-customer_gen_2022.111.64\output\modem\image\modem_pr";

// The destination folder for the inf package
string infDestinationFolder = "MCFG";

// If you use below's option, you'll want to output renamed files somewhere.
string output = @"E:\NewModem";

// Typically used for drops with sub folders, like MCFG
bool renameFiles = true;

/*
 * Code
 */

static string ComputeSha256Hash(string filePath)
{
    string filename = Path.GetFileName(filePath).ToLower();
    StringBuilder stringBuilder = new();
    using SHA256 sha256 = SHA256.Create();
    Encoding enc = Encoding.UTF8;
    byte[] sha256filehash = sha256.ComputeHash(enc.GetBytes(filename));

    foreach (byte @byte in sha256filehash)
    {
        stringBuilder.Append(@byte.ToString("x2"));
    }

    string sha256hash = stringBuilder.ToString();
    return sha256hash;
}

List<string> outputFiles = new();

List<string> DestinationDirs = new();
List<string> SourceDisksFiles = new();
List<string> SUBSYS_Device_XXXX_ext_NT = new();
List<string> SOFiles = new();
List<string> HashMapping = new();

int counter = 0;
foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
{
    counter++;
    string remotePath = file.Replace(Path.PathSeparator, '\\').Replace(directory, remotePathDestinationFolder);
    string hostPath = renameFiles ? $"{Path.GetFileName(file)}.{counter}" : Path.GetFileName(file);
    outputFiles.Add(hostPath);

    string fileHash = "";
    if (renameFiles)
    {
        string destination = Path.Combine(output, hostPath);
        fileHash = ComputeSha256Hash(destination).ToLower();
        File.Copy(file, destination);
    }
    else
    {
        fileHash = ComputeSha256Hash(file).ToLower();
    }

    HashMapping.Add($"; Updating registry of {hostPath} to remote path {remotePath}");
    HashMapping.Add($@"HKR,Mappings\{protocol}\Default\{fileHash},""Local"",%REG_SZ%, %13%\{infDestinationFolder}\{hostPath}");
    HashMapping.Add($@"HKR,Mappings\{protocol}\Default\{fileHash},""Remote"",%REG_SZ%, {remotePath}".ToLower());
    HashMapping.Add("");
}

outputFiles.Reverse();

string CopyFiles = "CopyFiles=" + string.Join(",", outputFiles);
SUBSYS_Device_XXXX_ext_NT.Add(CopyFiles);

foreach (var f in outputFiles)
{
    SOFiles.Add($"[{f}]");
    SOFiles.Add(f);
}

foreach (var f in outputFiles)
{
    SourceDisksFiles.Add($@"{f} = 1,\{infDestinationFolder}");
}

foreach (var f in outputFiles)
{
    DestinationDirs.Add($@"{f} = 13,\{infDestinationFolder}");
}

/*
 * Display data collected, it is divided into 5 sections, corresponding to one section in the inf
 * These are in order of the inf file.
 */
foreach (string line in DestinationDirs)
{
    Console.WriteLine(line);
}

Console.WriteLine("\n-------------------------------\n");

foreach (string line in SourceDisksFiles)
{
    Console.WriteLine(line);
}

Console.WriteLine("\n-------------------------------\n");

foreach (string line in SUBSYS_Device_XXXX_ext_NT)
{
    Console.WriteLine(line);
}

Console.WriteLine("\n-------------------------------\n");

foreach (string line in SOFiles)
{
    Console.WriteLine(line);
}

Console.WriteLine("\n-------------------------------\n");

foreach (string line in HashMapping)
{
    Console.WriteLine(line);
}