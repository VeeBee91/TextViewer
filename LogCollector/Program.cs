using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;

if (args.Length > 0 && args[0] == "p") 
{
    string password = "";
    while (true)
    {
        var key = System.Console.ReadKey(true);
        if (key.Key == ConsoleKey.Enter)
            break;
        password += key.KeyChar;
    }
    byte[] entropy = new byte[16];
    new RNGCryptoServiceProvider().GetBytes(entropy);
    byte[] bytes = Encoding.ASCII.GetBytes(password);
    File.WriteAllText("assist.d", Convert.ToBase64String(entropy) + "-" + Convert.ToBase64String(ProtectedData.Protect(bytes, entropy, DataProtectionScope.CurrentUser)));
    return;
}

bool nNF = false;
if (File.Exists("errorlog.txt"))
{
    var log = File.ReadAllLines("errorlog.txt");
    if(log.Length > 1)
        nNF = log.Last().Contains("No new files");
}
using StreamWriter sw = new("errorlog.txt", true);
string filePath = "logs";
string from = "";
string to = "";
int fileCount = 0;
List<string> settings = new();
if (File.Exists("settings.ini"))
{
    settings = File.ReadAllLines("settings.ini").ToList();
    filePath = settings[0];
    if (settings.Count > 1)
    {
        fileCount = int.Parse(settings[1]);
        if (settings.Count > 2)
        {
            from = settings[2];
            to = settings[3];
        }
    }
}  
var files = Directory.GetFiles(filePath);
if (files.Length > fileCount)
{
    sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : counted files: " + files.Length + ". Starting foreach loop.");
    SortedDictionary<DateTime, string> sd = new();
    foreach (var file in files)
    {
        if (!file.Contains("LogCollector"))
        {
            var s = File.ReadAllText(file);
            var test = s[..21];
            if ((!string.IsNullOrWhiteSpace(s)) && DateTime.TryParse(test, out DateTime r))
            {
                while (sd.ContainsKey(r))
                    r = r.AddMilliseconds(1);
                sd.Add(r, s.Replace("\r\n", ""));
            }
        }
    }

    sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : counted entries in dict: " + sd.Count + ". Writing to file.");
    using (StreamWriter dst = new("LogCollector.log"))
    {
        foreach (var entry in sd)
            dst.WriteLine(entry.Value);
    }
    if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
    {
        sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : Email settings found.");
        string[] dec = File.ReadAllText("assist.d").Split('-');
        SmtpClient client = new("smtp-mail.outlook.com", 587);
        client.EnableSsl = true;
        client.Credentials = new System.Net.NetworkCredential(from, Encoding.ASCII.GetString(ProtectedData.Unprotect(Convert.FromBase64String(dec[1]), Convert.FromBase64String(dec[0]), DataProtectionScope.CurrentUser)));
        MailMessage msg = new(from, to, "New LogCollector Update", "Attached file for info.");
        msg.Attachments.Add(new Attachment("LogCollector.log", MediaTypeNames.Application.Octet));
        try
        {
            client.Send(msg);
            sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : Mail send.");
        }
        catch (Exception ex)
        {
            sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : Exception sending mail : " + ex.Message);
            if (ex.InnerException != null)
                sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : InnerException: " + ex.InnerException.Message);
        }
    }
    if (settings.Count > 1)
    {
        settings[1] = files.Length.ToString();
    }
    else
    {
        settings.Add(files.Length.ToString());
    }

    sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : Updating settings file.");

    File.WriteAllLines("settings.ini", settings);
}
else
{
    if(!nNF)
        sw.WriteLine(DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + " : No new files.");
}