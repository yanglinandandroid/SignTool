using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SignTool
{
    public partial class MainForm : Form
    {
       // private string LocalAppPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 12);
        private List<Dictionary<string, string>> DeviceTypes = new List<Dictionary<string, string>>();
        private List<string> signFilePaths = new List<string>();//签名文件存放的集合
        private string selectSignToolFilePath = string.Empty;//设备类型选中时对应的签名包路径
        private static string APKNAME = "moons-xst-app-track.apk";
        private static string APKSIGNEDNAME = "moons-xst-app-track_signed.apk";
        private Dictionary<string, string> items;
        string[] spiltOriginalNameArray = null;
        string saveSignedAPKPath = string.Empty;//APK最终存放的路径
        string basePath;

        public MainForm()
        {
          
            InitializeComponent();
           
        }

        private void loadSignToolURL()
        {
            Environment.CurrentDirectory = System.Windows.Forms.Application.ExecutablePath .Substring(0, System.Windows.Forms.Application.ExecutablePath.Length-12);
            // Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory + @"DownLoad\";
            basePath = Environment.CurrentDirectory;
            DirectoryInfo directoryInfo = new DirectoryInfo(basePath + @"\DownLoad\");
            DirectoryInfo[] directoryInfos =directoryInfo.GetDirectories();
            for (int i = 0; i < directoryInfos.Length; i++) {
                signFilePaths.Add(basePath + @"\DownLoad\"+directoryInfos[i].ToString());
            }

        }
        private void loadDeivceType(ref string emsg)
        {
             items = GetDeivceTypes();
            if (items == null)
            {
                return;
            }

        }

        private Dictionary<string, string> GetDeivceTypes()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlNodeList nodeList = GetNodeList(doc);
                foreach (XmlNode node in nodeList)
                {
                    dic.Add(node.Attributes["Name"].InnerText.Trim(), node.Attributes["Link"].InnerText.Trim());
                }
            }
            catch
            {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "签名出错！";
                return null;
            }

            return dic;

        }
        XmlNodeList GetNodeList(XmlDocument doc)
        {
            if (!File.Exists(basePath + @"\DeviceTypeConfig.xml"))
            {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "缺少配置文件！";
                return null;
            }
            else
                doc.Load(basePath + @"\DeviceTypeConfig.xml");

            XmlNode baseNode = doc.SelectSingleNode("DeviceTypeConfig");
            if (baseNode == null)
            {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "配置文件内容为空！";
                return null;
            }
            return baseNode.SelectNodes("DeviceType");
        }

        //签名
        private void btn_sign_Click(object sender, EventArgs e)
        {
            sign();
        }


        private void sign()
        {
            Process proc = null;
            string baseSignToolPath = string.Empty;
            string emsg = string.Empty;
            Boolean isHave = false;
            loadSignToolURL();
            loadDeivceType(ref emsg);
            if (!string.IsNullOrEmpty(this.label3.Text))
            {
                this.label3.Text = string.Empty;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "请先选择要签名的APK文件！";
                return;
            }

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择一个文件夹";
            folderBrowserDialog.ShowNewFolderButton = false;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
               saveSignedAPKPath = folderBrowserDialog.SelectedPath;
            }
            if (string.IsNullOrEmpty(saveSignedAPKPath))
            {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "请选择要存放的位置！";
                return;
            }

            string originalName = this.textBox1.Text.Substring(this.textBox1.Text.LastIndexOf(@"\"));
            spiltOriginalNameArray = originalName.Split(new char[1] { '_' });
            if (spiltOriginalNameArray.Length != 4) {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "APK文件命名错误！";
                return;
            }
            string deviceType = spiltOriginalNameArray[3].Substring(0, spiltOriginalNameArray[3].Length - 4);


          
            foreach (var item in items)
            {
                if (item.Key.Equals(deviceType))
                {
                    isHave = true;
                    try
                    {
                        File.Move(this.textBox1.Text, basePath + @item.Value + @originalName);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                    selectSignToolFilePath = @item.Value;
                    break;
                }

            }
            //设备名称不对
            if (!isHave) {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "APK文件命名错误！";
                return;
            }
            if (this.label3.Text.Equals("APK文件命名错误！")) {
                return;
            }
           
            try
            {
                Environment.CurrentDirectory = System.Windows.Forms.Application.ExecutablePath.Substring(0, System.Windows.Forms.Application.ExecutablePath.Length - 12) + @"\" + @selectSignToolFilePath;

                baseSignToolPath = basePath+ selectSignToolFilePath;

            firstReName(baseSignToolPath,originalName);
            
                proc = new Process();
               // this.label3.Text = "开启签名进程    " + Environment.CurrentDirectory;
                proc.StartInfo.FileName = Environment.CurrentDirectory + @"\Sign.bat";
                proc.StartInfo.Arguments = string.Format("10");
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();


            FileInfo signedFileInfo = new FileInfo(baseSignToolPath + @"\"+APKSIGNEDNAME);
            if (signedFileInfo.Exists)
            {
                proc.Close();
                proc.Dispose();
                FileInfo fileInfo = new FileInfo(baseSignToolPath + @"\" + APKNAME);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
                secondReName(baseSignToolPath,APKSIGNEDNAME);
            }
            else {
                this.label3.ForeColor = Color.Red;
                this.label3.Text = "签名出错！";
            }
            }
            catch (Exception ex)
            {
             
                proc.Close();
                proc.Dispose();

                this.label3.ForeColor = Color.Red;
                this.label3.Text = "签名出错！";
                return;
            }

        }

        private void firstReName(string basePath,string originalName)
        {
            File.Move(@basePath+@"\"+ originalName, @basePath + @"\" +APKNAME);
        }

        private void secondReName(string basePath, string originalName)
        {
            string jointPath = @"\moons-xst-app-track_" + spiltOriginalNameArray[1] + "_" + spiltOriginalNameArray[2] + "(" + spiltOriginalNameArray[3].Substring(0,spiltOriginalNameArray[3].Length-4) + ").apk";
            string lastSignedFilePath = basePath + jointPath;
            File.Move(basePath + @"\" + APKSIGNEDNAME, lastSignedFilePath);
            FileInfo lastSignedFileInfo = new FileInfo(lastSignedFilePath);
            if (lastSignedFileInfo.Exists) {
                lastSignedFileInfo.CopyTo(saveSignedAPKPath + jointPath,true);
                lastSignedFileInfo.Delete();
            }
            this.label3.ForeColor = Color.Blue;  
            this.label3.Text = "签名完成！";
            this.textBox1.Text = string.Empty;
        }

        //浏览
        private void btn_select_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.label3.Text))
            {
                this.label3.Text = string.Empty;
            }
            if (!string.IsNullOrEmpty(this.textBox1.Text))
            {
                this.textBox1.Text = string.Empty;
            }
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "APK文件(*.apk)|*.apk";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = fileDialog.FileName;
                fileDialog.Dispose();
                fileDialog = null;
            }

        }
        //程序退出
        private void Form1_Load(object sender, EventArgs e)
        {
            //  Application.Exit();
        }
    }
}
