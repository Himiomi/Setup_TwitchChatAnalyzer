using System;
using System.IO;
using System.Windows.Forms;
using System.Management.Automation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InstallerAnalyzer
{
    public partial class Form1 : Form
    {

        string[] listImageNecessaire =
        {
                "hikachuu/grafana",
                "hikachuu/java_getter",
                "hikachuu/truc",
                "hikachuu/sql_data"
            };
        string[] listImageNecessaireRef =
        {
                "hikachuu/grafana",
                "hikachuu/java_getter",
                "hikachuu/truc",
                "hikachuu/sql_data"
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
            button3.Visible=false;
            button2.Enabled=false;

            CheckIfExist();
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
            foreach (string imageName in listImageNecessaire) {
                var script = "docker pull " +imageName+":latest";

                var powerShell = PowerShell.Create().AddScript(script);
                foreach (PSObject item in powerShell.Invoke().ToArray())
                {
                    Debug.Write(item.ToString());
                }
            }
            listView1.Items.Clear();
            listView2.Items.Clear();
            listImageNecessaire = listImageNecessaireRef;
            verifImage();
        }

        private void verifImage()
        {
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
            foreach (string current in listImageNecessaire)
            {
                string[] listElem = { current, "latest" };
                var listViewItem = new ListViewItem(listElem);
                listView2.Items.Add(listViewItem);

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ThreadStart method = new ThreadStart(DoWork);

            // Créer un nouveau thread et le démarrer
            Thread newThread = new Thread(method);
            newThread.Start();
            var script = "docker ps -a --filter \"ancestor=hikachuu/grafana:latest\" --format \"{{.Status}}\" | Select-String -Pattern \"Up\"";
            string result = "";
            do
            {
                result=String.Join("|", PowerShell.Create().AddScript(script).Invoke().ToList());
                Debug.Write(result+"\n");

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
            listView4.Items.Add(listViewItem);
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

        }
    }
}
