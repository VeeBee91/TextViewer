using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace TextViewer.Classes
{
    public class Logic
    {
        internal static Tuple<List<string>, bool> AppendArray(Queue<string> queue, int count, List<string>? source = null)
        {
            int c = count;
            bool ponr = false;
            List<string> data = new();
            if (source != null)
                data = source.ToList();
            Log("AppendArray started with " + queue.Count + " items in queue, " + count + " items to process, " + data.Count + " inherited lines.");
            if (queue.Peek().Contains("---------------------------------------"))
                ponr = true;
            for (int i = 0; i < count; i++)
                if (!ponr)
                {
                    if (queue.Peek().Contains("---------------------------------------"))
                    {
                        ponr = true;
                        break;
                    }
                    data.Add(queue.Dequeue());
                }
                else
                    data.Add(queue.ToArray()[i]);
            Log("AppendArray Completed. New result has " + data.Count + " items, and ponr is " + ponr + ".");
            return new Tuple<List<string>, bool>(data, ponr);
        }

        internal static async Task<bool> VerifyResponse(List<string> answers)
        {
            Log("Start VerifyResponse.");
            var raw = await File.ReadAllLinesAsync(Path.Combine("data", "verify.d"));
            List<string[]> data = new();
            foreach (string r in raw)
                data.Add(r.Split(',').Select(d => d.Trim().ToLower()).ToArray());
            for (int i = 0; i < answers.Count; i++)
                if (!data[i].Any(answers[i].Trim().ToLower().Contains))
                {
                    Log("Answer " + answers[i] + " is not correct.");
                    return false;
                }
            return true;
        }

        internal static async Task<string> VerifyAccess()
        {
            var s = await File.ReadAllTextAsync(Path.Combine("data", "confirm.d"));
            if (s.Contains("true"))
            {
                try
                {
                    await File.WriteAllTextAsync(Path.Combine("data", "confirm.d"), "false");
                }
                catch { }
                return "../api/12B4499B-4827-40C3-80F7-491C31C96973";
            }
            else
                return "../api/840F343C-69EF-4C53-BEAD-B4AA243B9D10";
        }

        internal static async Task ClearSource(Queue<string> queue, IDataProtectionProvider provider, bool ponr = false)
        {
            Log("Replacing source.");
            var encryptor = new Encryptor(provider);
            StringBuilder sb = new();
            if (ponr)
            {
                var encData = await File.ReadAllTextAsync(Path.Combine("data", "data.d"));
                var data = encryptor.Decrypt(encData).Split(new String[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (data[0].Contains("---------------------------------------"))
                    return;
            }
            foreach (var item in queue)
                sb.AppendLine(item.Replace("<br />", ""));
            await File.WriteAllTextAsync(Path.Combine("data", "data.d"), encryptor.Encrypt(sb.ToString()));
        }

        internal static async Task<bool> SaveData(string data, IDataProtectionProvider provider)
        {
            try
            {
                Log("Encrypting data.");
                var encryptor = new Encryptor(provider);
                Log("Writing encrypted data to file.");
                await File.WriteAllTextAsync(Path.Combine("data", "data.d"), encryptor.Encrypt(data));
                Log("Data file saved succesfully.");
                return true;
            }
            catch (Exception ex)
            {
                Log("Error saving data file. Exception: " + ex.Message);
                return false;
            }
        }

        internal static async Task<Queue<string>> ReadAndProcessSource(IDataProtectionProvider provider)
        {
            var encryptor = new Encryptor(provider);
            var encData = await File.ReadAllTextAsync(Path.Combine("data", "data.d"));
            var data = encryptor.Decrypt(encData).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Log("Reading data. Lenght = " + data.Length);
            List<string> vs = new();
            foreach (string line in data)
                if (line.Length > 150)
                {
                    int d = 2;
                    int r = line.Length / d;
                    string temp = line;
                    while (r > 150)
                    {
                        d++; r = line.Length / d;
                    }
                    var a = line.Trim().Split(". ");
                    int index = 0;
                    while (index < a.Length)
                    {
                        string text = "";
                        while (text.Length < r - 1 && index < a.Length - 1)
                        {
                            text += a[index] + ". ";
                            index++;
                        }
                        string eol = "";
                        if (a[index].Length > 5)
                            eol = a[index][^4..];
                        else
                            eol = a[index];
                        if (((!eol.Contains('.')) && (!eol.Contains('?'))) || eol.Contains(".."))
                            text += a[index] + ". ";
                        else
                            text += a[index];
                        index++;
                        temp = temp.Replace(text.Trim(), "");
                        if (index == a.Length)
                        {
                            if ((!string.IsNullOrWhiteSpace(temp)) && temp.Trim() != ".")
                                text += " " + temp;
                            text += " <br />";
                        }
                        vs.Add(text);
                    }
                }
                else
                    vs.Add(line + " <br />");
            Log("Resulting queue is " + vs.Count + " long.");
            return new Queue<string>(vs);
        }

        internal static async Task Log(string log)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            var file = Path.GetRandomFileName();
            while (File.Exists(Path.Combine("logs", file)))
                file = Path.GetRandomFileName();
            using StreamWriter w = File.AppendText(Path.Combine("logs", file));
            if (string.IsNullOrWhiteSpace(log) == false)
                await w.WriteLineAsync(DateTime.Now.ToString("dd/MM/yy HH:mm:ss.fff") + ": " + log.Replace("\0", ""));
        }

        internal static string RouteFilter(string UserAgent, string IPAddress, HttpRequest request)
        {
            if (!File.ReadAllText(Path.Combine("data", "wl.d")).Trim().Contains(IPAddress))
            {
                Log("Visitor not whitelisted.");
                string[] AllowedBrowsers = new[] { "android", "webos", "iphone", "ipod", "blackberry", "iemobile", "opera mini", "mobile" };
                if (!AllowedBrowsers.Any(UserAgent.ToLower().Contains))
                {
                    Log("Agent is not a mobile browser. Send to errmob.");
                    return "/0B17E5F2-AEDE-40BA-8BFE-4D2F3B677023";
                }

                if (!File.Exists(Path.Combine("data","data.d")))
                {
                    Log("No data file uploaded. Send to nrdy.");
                    return "/79B53C1A-9C6E-48AD-9622-3A757B4ADA7B";
                }

                if (File.Exists(Path.Combine("data", "lock.d")))
                {
                    Log("Lock file exists.");
                    var authToken = request.Cookies[".AspNetCore.Cookies"];
                    if (authToken == null)
                    {
                        Log("Client does not have the cookie. Send to frbdn");
                        return "/6FA48EDA-E265-40B9-B4B5-9031C7A05DF8";
                    }
                    else if (!request.Path.Value.Contains("35DC6E98-334D-4F26-A5DD-07AB233EA3A8"))
                    {
                        Log("Client has cookie but did not connect with reader. Sending to reader.");
                        return "/app/35DC6E98-334D-4F26-A5DD-07AB233EA3A8";
                    }
                }
                else
                {
                    Log("No lock yet.");
                    if (request.Path.Value.Contains("35DC6E98-334D-4F26-A5DD-07AB233EA3A8"))
                    {
                        Log("Unauthenticated client tried to get to reader. Redirected to check.");
                        return "/app/3EB5C6FB-5D0D-4238-8A8D-0269DA976662";
                    }
                }
            }

            return "";

        }

        internal static async Task<bool> DarkMode(bool write = false)
        {
            if (write)
                await File.WriteAllTextAsync(Path.Combine("data", "darkmode.d"), "true");
            else
            {
                if (!File.Exists(Path.Combine("data", "darkmode.d")))
                    return false;
            }
            return true;
        }

        internal static async Task<List<string>> ReadQuestions()
        {
            var data = await File.ReadAllLinesAsync(Path.Combine("data", "questions.d"));
            return data.ToList();
        }
    }
}
