using System;
using System.IO;
using System.Windows.Forms;
using System.Management.Automation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Data;
using MySql.Data.MySqlClient;

namespace InstallerAnalyzer
{
    public partial class Form1 : Form
    {

        string[] listImageNecessaire =
        {
                "hikachuu/grafana",
                "hikachuu/java_getter_data",
                "hikachuu/storagetwitch",
                "hikachuu/frontid"
            };
        string[] listImageNecessaireRef =
        {
                "hikachuu/grafana",
                "hikachuu/java_getter_data",
                "hikachuu/storagetwitch",
                "hikachuu/frontid"
            };

        public Form1()
        {
            InitializeComponent();

            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;
            listView3.View = View.Details;
            listView3.GridLines = true;
            listView3.FullRowSelect = true;
            listView4.View = View.Details;
            listView4.GridLines = true;
            listView4.FullRowSelect = true;
            button3.Visible = false;
            button2.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            CheckIfExist();
            verifImage();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            CheckIfExist();
        }

        private void CheckIfExist()
        {
            string softwareName = "Docker";
            string path = "C:\\Program Files\\" + softwareName;

            if (Directory.Exists(path))
            {
                label1.Text = softwareName + " est installé.";
                button2.Enabled = true;
            }
            else
            {
                label1.Text = softwareName + " n'est pas installé, vous pouvez l'installer en cliquant ici :";
                button3.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            verifImage();
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string url = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe";
            Process.Start(url);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (string imageName in listImageNecessaire)
            {
                var script = "docker pull " + imageName + ":latest";
                AddNotification("Début du téléchargement de " + imageName + ". Veuillez patientez");
                ExecInPowerShell(script);
            }
            listView1.Items.Clear();
            listView2.Items.Clear();
            listImageNecessaire = listImageNecessaireRef;
            verifImage();
        }

        private void verifImage()
        {
            listImageNecessaire = listImageNecessaireRef;
            listView1.Items.Clear();
            listView2.Items.Clear();
            var script = "docker images";

            var powerShell = PowerShell.Create().AddScript(script);
            string[] listImageInstalle = { };
            foreach (PSObject item in powerShell.Invoke().ToArray().Skip(1))
            {
                string pattern = @"^(\S+)\s+(\S+)\s+(\S+)";

                Match match = Regex.Match(item.ToString(), pattern);

                if (match.Success)
                {
                    if (listImageNecessaire.Contains(match.Groups[1].Value))
                    {
                        string[] listElem = { match.Groups[1].Value, match.Groups[2].Value };
                        var listViewItem = new ListViewItem(listElem);
                        listView1.Items.Add(listViewItem);
                        listImageNecessaire = listImageNecessaire.Where(a => a != match.Groups[1].Value).ToArray();
                    }
                }
                else
                {
                    Console.WriteLine("Le texte ne correspond pas au modèle.");
                }
            }
            if (listImageNecessaire.Length == 0)
            {
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
            }
            foreach (string current in listImageNecessaire)
            {
                string[] listElem = { current, "latest" };
                var listViewItem = new ListViewItem(listElem);
                listView2.Items.Add(listViewItem);

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Thread newThread = new Thread(new ThreadStart(DoWork));
            Thread newThread1 = new Thread(new ThreadStart(CheckGrafana));
            Thread newThread2 = new Thread(new ThreadStart(CheckFrontId));
            Thread newThread3 = new Thread(new ThreadStart(CheckJavaGetter));
            Thread newThread4 = new Thread(new ThreadStart(CheckMySQL));
            Thread newThread5 = new Thread(new ThreadStart(CheckAll));
            newThread.Start();
            newThread1.Start();
            newThread2.Start();
            newThread3.Start();
            newThread4.Start();
            newThread5.Start();
            var script = "docker ps -a --filter \"ancestor=hikachuu/grafana:latest\" --format \"{{.Status}}\" | Select-String -Pattern \"Up\"";
            string result = "";
            do
            {
                result = String.Join("|", PowerShell.Create().AddScript(script).Invoke().ToList());
                Debug.Write(result + "\n");

                Thread.Sleep(1000);
            } while (result.Length < 2);

            AddNotification("Projet correctement lancé");
            RefreshActivities();
        }
        public void DoWork()
        {
            var script = "docker compose up";
            var powerShell = PowerShell.Create().AddScript(script).Invoke();

        }

        private void CheckGrafana()
        {
            bool status = false;
            do
            {
                Thread.Sleep(1000);
                var hostUrl = "http://localhost:3000/d/qCwW_Uh4k/analyse-replay-twitch?orgId=1";
                status = CheckUrlStatus(hostUrl);
                Debug.Write("aled " + status);
            } while (status);
            label12.Invoke(new MethodInvoker(delegate
            {
                label12.Text = "OK";
            }));
            Debug.Write("OK");
        }
        private void CheckFrontId()
        {
            bool status = false;
            do
            {
                Thread.Sleep(1000);
                var hostUrl = "http://localhost:4200";
                status = CheckUrlStatus(hostUrl);
                Debug.Write("aled " + status);
            } while (!status);
            label14.Invoke(new MethodInvoker(delegate
            {
                label14.Text = "OK";
            }));
            Debug.Write("OK");
        }
        private void CheckJavaGetter()
        {
            bool status = false;
            do
            {
                Thread.Sleep(1000);
                var hostUrl = "http://localhost:8080/home/";
                status = CheckUrlStatus(hostUrl);
                Debug.Write("aled " + status);
            } while (!status);
            label13.Invoke(new MethodInvoker(delegate
            {
                label13.Text = "OK";
            }));
            Debug.Write("OK");
        }
        private void CheckMySQL()
        {
            bool status = true;
            do
            {
                Thread.Sleep(1000);

                try
                {
                    string serverName = "localhost"; // Address server (for local database "localhost")
                    string userName = "root";  // user name
                    string dbName = "test"; //Name database
                    string port = "3306"; // Port for connection
                    string password = "my-secret-pw"; // Password for connection 
                    string conStr = "server=" + serverName +
                               ";user=" + userName +
                               ";database=" + dbName +
                               ";port=" + port +
                               ";password=" + password + ";";

                    using (MySqlConnection con = new MySqlConnection(conStr))
                    {
                        string sql0 = "show tables;"; //any request for cheking
                        MySqlCommand cmd0 = new MySqlCommand(sql0, con);
                        con.Open();
                        cmd0.ExecuteScalar();
                        con.Close();
                        status = false;
                    }
                }
                catch (Exception)
                {
                    status=true;
                }
                Debug.Write("MySQL " + status);

            } while (!status);
            Debug.Write("OK MySQL");

            label15.Invoke(new MethodInvoker(delegate
            {
                label15.Text = "OK";
            }));
        }
        private void CheckAll()
        {
            do
            {
                Thread.Sleep(1000);
                
            } while (!(label12.Text=="OK" && label13.Text == "OK" && label14.Text == "OK" && label15.Text == "OK"));
            label18.Invoke(new MethodInvoker(delegate
            {
                label18.Text = "OK";
            }));
            Debug.Write("All OK");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RefreshActivities();
        }

        private void RefreshActivities()
        {
            var script = "docker ps";

            var powerShell = PowerShell.Create().AddScript(script);

            listImageNecessaire = listImageNecessaireRef;
            listView3.Items.Clear();
            foreach (PSObject item in powerShell.Invoke().ToArray().Skip(1))
            {
                Debug.Write(item.ToString() + "\n");
                string pattern = @"^(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)";
                string pattern2 = @".*ago\s+(\S+)\b";
                Match match = Regex.Match(item.ToString(), pattern);
                Match match2 = Regex.Match(item.ToString(), pattern2);

                if (match.Success && match2.Success)
                {
                    string[] listElem = { match.Groups[2].Value, match2.Groups[1].Value };

                    var listViewItem = new ListViewItem(listElem);
                    listView3.Items.Add(listViewItem);
                    listImageNecessaire = listImageNecessaire.Where(a => !match.Groups[2].Value.Contains(a)).ToArray();
                }
                else
                {
                    Console.WriteLine("Le texte ne correspond pas au modèle.");
                }
            }

            foreach (string element in listImageNecessaire)
            {
                string[] listElem = { element, "Down" };

                var listViewItem = new ListViewItem(listElem);
                listView3.Items.Add(listViewItem);
            }
        }

        private void AddNotification(string message)
        {

            string[] listElem = { message };

            var listViewItem = new ListViewItem(listElem);
            listView4.Invoke(new MethodInvoker(delegate
            {
                listView4.Items.Add(listViewItem);
            }));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ThreadStart method = new ThreadStart(IncredibleDoWork);

            // Créer un nouveau thread et le démarrer
            Thread newThread = new Thread(method);
            newThread.Start();
            var script = "docker ps -a --filter \"ancestor=hikachuu/grafana:latest\" --format \"{{.Status}}\" | Select-String -Pattern \"Up\"";
            string result = "";
            do
            {
                result = String.Join("|", PowerShell.Create().AddScript(script).Invoke().ToList());
                Debug.Write(result + "\n");

                Thread.Sleep(1000);
            } while (!(result.Length < 2));

            AddNotification("Projet correctement arrêté lancé");
            RefreshActivities();

        }
        public void IncredibleDoWork()
        {
            var script = "docker compose down";
            var powerShell = PowerShell.Create().AddScript(script).Invoke();

            label12.Invoke(new MethodInvoker(delegate
            {
                label12.Text = "KO";
            }));
            label13.Invoke(new MethodInvoker(delegate
            {
                label13.Text = "KO";
            }));
            label14.Invoke(new MethodInvoker(delegate
            {
                label14.Text = "KO";
            }));
            label15.Invoke(new MethodInvoker(delegate
            {
                label15.Text = "KO";
            }));
            label18.Invoke(new MethodInvoker(delegate
            {
                label18.Text = "KO";
            }));

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://localhost:4200");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://localhost:3000/d/qCwW_Uh4k/analyse-replay-twitch?orgId=1");

        }

        private void ExecInPowerShell(string command)
        {
            Process p = new Process();
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = @"powershell.exe";
            p.StartInfo.Arguments = $"" + command;

            p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (e.Data != null)
                {
                    if (e.Data.Contains("Status: Downloaded newer image for")) AddNotification("Tâche terminée correctement");
                }
                Console.WriteLine(e.Data);
            });
            p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                Console.WriteLine(e.Data);
            });

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ExecInPowerShell(" docker images ; \"SLEEP 2\"");
        }
        protected bool CheckUrlStatus(string Website)
        {
            try
            {
                var request = WebRequest.Create(Website) as HttpWebRequest;
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Debug.Write("State =" + response.StatusCode);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            label12.Text = "KO";
            label13.Text = "KO";
            label14.Text = "KO";
            label15.Text = "KO";
            label18.Text = "KO";
            Thread.Sleep(1000);
            Thread newThread1 = new Thread(new ThreadStart(CheckGrafana));
            Thread newThread2 = new Thread(new ThreadStart(CheckFrontId));
            Thread newThread3 = new Thread(new ThreadStart(CheckJavaGetter));
            Thread newThread4 = new Thread(new ThreadStart(CheckMySQL));
            Thread newThread5 = new Thread(new ThreadStart(CheckMySQL));
            newThread1.Start();
            newThread2.Start();
            newThread3.Start();
            newThread4.Start();
            newThread5.Start();
        }
    }
}
