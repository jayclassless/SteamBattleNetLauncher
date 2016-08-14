using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace SteamBattleNetLauncher {
    static class Program {
        static Dictionary<string, string[]> GAME_TOKEN_MAP = new Dictionary<string, string[]>(){
            {"D3", new string[]{"D3", "Diablo 3"}},
            {"HERO", new string[]{"Hero", "Heroes of the Storm"}},
            {"PRO", new string[]{"Pro", "Overwatch"}},
            {"S2", new string[]{"S2", "Starcraft 2"}},
            {"WOW", new string[]{"WoW", "World of Warcraft"}},
            {"WTCG", new string[]{"WTCG", "Hearthstone"}},
        };

        static int PROCESS_WAIT_LIMIT = 10000;
        static int PROCESS_WAIT_INCREMENT = 250;

        [STAThread]
        static void Main(string[] argv) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appTitle = "Steam Battle.Net Launcher";

            // Validate input parameter.
            string gameToken;
            if ((argv.Length > 0) && (GAME_TOKEN_MAP.ContainsKey(argv[0].ToUpper()))) {
                gameToken = GAME_TOKEN_MAP[argv[0].ToUpper()][0];
            } else {
                StringBuilder message = new StringBuilder("This program must be launched with one of the following parameters:\n");
                foreach (var kvp in GAME_TOKEN_MAP) {
                    message.AppendLine(String.Format("\t{0}\t({1})", kvp.Key, kvp.Value[1]));
                }
                MessageBox.Show(message.ToString(), appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FrmStatus statusWindow = new FrmStatus();
            statusWindow.Show();
            statusWindow.UpdateStatus("Requested launch of: {0}", GAME_TOKEN_MAP[gameToken.ToUpper()][1]);

            // Make sure Battle.Net is running.
            int battleNetProcessID = GetBattleNetProcessID();
            if (battleNetProcessID == 0) {
                statusWindow.UpdateStatus("Battle.Net not running, starting it...");
                Process.Start("battlenet://");
            }
            statusWindow.UpdateStatus("Found Battle.Net Process: {0}", battleNetProcessID);

            // Make sure the Helpers are running.
            int numHelpers = GetHelperCount(battleNetProcessID),
                timeWaited = 0;
            while ((numHelpers < 2) && (timeWaited < PROCESS_WAIT_LIMIT)) {
                Thread.Sleep(PROCESS_WAIT_INCREMENT);
                timeWaited += PROCESS_WAIT_INCREMENT;
                numHelpers = GetHelperCount(battleNetProcessID);
            }
            if (numHelpers < 2) {
                MessageBox.Show(
                    "The Battle.Net Helper processes could not be found. Try completely shutting down Battle.Net and try again.",
                    appTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            statusWindow.UpdateStatus("Found Battle.Net Helpers.");

            // Make Battle.Net start the game.
            statusWindow.UpdateStatus("Analyzing game launch information...");
            Process.Start(String.Format("battlenet://{0}", gameToken));
            int gameProcessId = 0;
            timeWaited = 0;
            while ((gameProcessId == 0) && (timeWaited < PROCESS_WAIT_LIMIT)) {
                gameProcessId = GetGameProcessID(battleNetProcessID);
                Thread.Sleep(PROCESS_WAIT_INCREMENT);
                timeWaited += PROCESS_WAIT_INCREMENT;
            }
            if (gameProcessId == 0) {
                MessageBox.Show(
                    "The game process took too long to launch.",
                    appTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Capture the info we need to start the game ourselves.
            Process process = new Process();
            process.StartInfo = GetProcessStartInfo(gameProcessId);
            statusWindow.UpdateStatus("Game executeable: {0}", process.StartInfo.FileName);
            statusWindow.UpdateStatus("Game arguments: {0}", process.StartInfo.Arguments);

            // Kill the game that was started by Battle.Net.
            if (gameToken == "Hero") {
                // This is hacky, but if we kill Heroes of the Storm too fast, it complains.
                Thread.Sleep(3000);
            }
            Process.GetProcessById(gameProcessId).Kill();
            statusWindow.UpdateStatus("Cleaned up analysis.");

            // Start the game under our ownership.
            statusWindow.UpdateStatus("Starting game...");
            process.Start();
        }

        private static int GetBattleNetProcessID() {
            string query = "SELECT ProcessId FROM Win32_process WHERE Name = 'Battle.Net.exe'";

            using (var mos = new ManagementObjectSearcher(query)) {
                foreach (var result in mos.Get()) {
                    return Convert.ToInt32(result["ProcessId"]);
                }
            }

            return 0;
        }

        private static int GetHelperCount(int battleNetProcessID) {
            string query = String.Format(
                "SELECT ProcessId FROM Win32_Process WHERE Name = 'Battle.net Helper.exe' AND ParentProcessId = {0}",
                battleNetProcessID
            );

            using (var mos = new ManagementObjectSearcher(query)) {
                return mos.Get().Count;
            }
        }

        private static int GetGameProcessID(int battleNetProcessID) {
            string query = String.Format(
                "SELECT ProcessId FROM Win32_Process WHERE Name <> 'Battle.net Helper.exe' AND ParentProcessId = {0}",
                battleNetProcessID
            );

            using (var mos = new ManagementObjectSearcher(query)) {
                foreach (var result in mos.Get()) {
                    return Convert.ToInt32(result["ProcessId"]);
                }
            }

            return 0;
        }

        private static ProcessStartInfo GetProcessStartInfo(int processId) {
            ProcessStartInfo startInfo = null;
            string query = String.Format("SELECT ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId = {0}", processId);

            using (var mos = new ManagementObjectSearcher(query)) {
                foreach (var result in mos.Get()) {
                    startInfo = new ProcessStartInfo();

                    startInfo.FileName = result["ExecutablePath"].ToString();
                    startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(startInfo.FileName);

                    string commandLine = result["CommandLine"].ToString();
                    int trimLength = startInfo.FileName.Length;
                    if (commandLine.StartsWith("\"")) {
                        trimLength += 2;
                    }
                    startInfo.Arguments = commandLine.Substring(trimLength).Trim();

                    return startInfo;
                }
            }

            return startInfo;
        }
    }
}
