using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

CountdownEvent Countdown;
int upCount = 0;
object lockObj = new object();
const bool resolveNames = true;



Countdown = new CountdownEvent(1);
Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();


string ip1 = GetLocalIPAddress();

string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            Console.WriteLine(ip.ToString());
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}

string ipBese = CutIP(ip1);

string CutIP(string ip)
{
    StringBuilder stringBuilder = new StringBuilder();
    string[] IP = ip.Split('.');
    for (int i = 0; i < 3; i++)
    {
        stringBuilder.Append(IP[i] + ".");
    }
    return stringBuilder.ToString();
}

//string ipBese = "10.0.0.";

for (int i = 0; i < 255; i++)
{
    string ip = ipBese + i.ToString();
    Ping p = new Ping();
    p.PingCompleted += new PingCompletedEventHandler(PPingCompleted);
    Countdown.AddCount();
    p.SendAsync(ip, 100, ip);
}

Countdown.Signal();
Countdown.Wait();
stopwatch.Stop();
TimeSpan span = new TimeSpan(stopwatch.ElapsedTicks);
Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds} milliseconds. {upCount} hosts active");
Console.ReadLine();






void PPingCompleted(object sender, PingCompletedEventArgs e)
{
    string ip = (string)e.UserState;
    if (e.Reply != null && e.Reply.Status == IPStatus.Success)
    {
        if (resolveNames)
        {
            string name;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                name = hostEntry.HostName;
            }
            catch (SocketException ex)
            {

                name = "?";
            }
            Console.WriteLine($"{ip} {name} is up: ({e.Reply.RoundtripTime} ms)");

        }
        else
        {
            Console.WriteLine($"{ip} is up: ({e.Reply.RoundtripTime} ms)");
        }
        lock (lockObj)
        {
            upCount++;
        }
    }
    else if (e.Reply == null)
    {
        Console.WriteLine($"pinging {ip} is faild");
    }
    Countdown.Signal();
}