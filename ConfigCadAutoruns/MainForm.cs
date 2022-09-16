using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigCadAutoruns
{
    public partial class MainForm : Form
    {
        public string InitFile { get; set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (!RegistrySetup.InstallByIni(InitFile))
            {
                this.MessageLabel.Text = string.Format("安装失败，请检查配置文件是否存在\n{0}", InitFile);
                return;
            }

            this.MessageLabel.Text = "安装成功。";
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            if (!RegistrySetup.UninstallByIni(InitFile))
            {
                this.MessageLabel.Text = string.Format("卸载失败，请检查配置文件是否存在\n{0}", InitFile);
                return;
            }

            this.MessageLabel.Text = "卸载成功。";
        }
    }
}
