using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
//====================================================================================================
//history:
//	2017.08.02 11:41 v1.0.0.1 update the access path of mp3Gain.exe, must be same path of this executable
//====================================================================================================

namespace myMp3Gain
{
	public partial class frmMain : Form
	{
		public frmMain()
		{
			InitializeComponent();
		}

		private string startUpPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			folderDlg.Description = "Select the directory that stores your mp3 files.";
			folderDlg.ShowNewFolderButton = false;
			DialogResult result = folderDlg.ShowDialog();
			if (result == DialogResult.OK) txtPath.Text = folderDlg.SelectedPath;
		}

		private void btnGo_Click(object sender, EventArgs e)
		{
			FileInfo fi = null;
			string orinFN = "";
			string tmpFullFN = "";
			int fileCnt = 0;
			//1.check if the mp3 folder is empty.
			if (txtPath.Text == "") { MessageBox.Show("no folder selected"); return; }
			string[] fileEntries = Directory.GetFiles(txtPath.Text);
			//2.do task on each file
			foreach (string fileName in fileEntries) {
				//2.1.get file information
				fi = new FileInfo(fileName);
				//2.2.only apply on mp3 files
				if (fi.Extension.ToString().ToLower() == ".mp3") {
					fileCnt++;
					//2.3.keep the oringinal filename in a temp string
					orinFN = fi.Name;
					//2.4.rename the file to temp filename withou unicode
					tmpFullFN = fileName.Replace(orinFN, "CVFGHB" + fileCnt.ToString() + ".mp3");
					fi.MoveTo(tmpFullFN);
					//2.5.do the task with new names
					processFile(tmpFullFN);
					tmpFullFN = tmpFullFN.Replace("CVFGHB" + fileCnt.ToString() + ".mp3", orinFN);
					fi.MoveTo(tmpFullFN);
				}
			}
			MessageBox.Show("done!!");
		}

		private double getGain2Apply(string fileName)
		{
			string res = "";
			string[] arrRes_1 = null;
			string[] arrRes_2 = null;
			double ret = 0.0d;
			Process p = new Process();
			// Redirect the output stream of the child process.
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = startUpPath + @"\mp3Gain.exe";
			p.StartInfo.Arguments = String.Format(" /s r \"{0}\"", fileName);
			p.Start();
			// Read the output stream first and then wait.
			res = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			arrRes_1 = res.Split('\n');
			arrRes_2 = arrRes_1[1].Split(':');
			ret = 5.0d + Convert.ToDouble(arrRes_2[1]);
			return ret;
		}

		private void processFile(string fileName)
		{
			string res = "";
			double gain2Apply = getGain2Apply(fileName);
			Process p = new Process();
			// Redirect the output stream of the child process.
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = startUpPath + @"\mp3Gain.exe";
			p.StartInfo.Arguments = String.Format(" /g {0} \"{1}\"", gain2Apply, fileName);
			p.Start();
			// Read the output stream first and then wait.
			res = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
		}
	}
}
