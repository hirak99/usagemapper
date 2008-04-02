using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UsageMapper
{
    public partial class Form1 : Form
    {
        public Form1() {
            InitializeComponent();
            AcceptButton = button1;
        }

        private void button1_Click(object sender, EventArgs e) {
            if (textBox1.Text.Trim().Length == 0) return;
            if (usageMapView1.CurrentFile == textBox1.Text) {
                if (MessageBox.Show("Do you want to rescan the current directory?", "Query",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    usageMapView1.fc.ForgetFolder(textBox1.Text);
                else return;
            }
            usageMapView1.CurrentFile = textBox1.Text;
        }

        private void usageMapView1_CurrentFileChanged(object Sender) {
            textBox1.Text = usageMapView1.CurrentFile;
        }

        private void button2_Click(object sender, EventArgs e) {
            if (textBox1.Text.Length == 0) return;
            string curFolder = textBox1.Text;
            if (curFolder[curFolder.Length - 1] == '\\') curFolder = curFolder.Substring(0, curFolder.Length - 1);
            int lastSlash = curFolder.LastIndexOf('\\');
            if (lastSlash>=0)
                curFolder = curFolder.Substring(0, lastSlash);
            if (curFolder.Length <= 3) curFolder += '\\';
            textBox1.Text = curFolder;
            usageMapView1.CurrentFile = curFolder;
        }

        private void button3_Click(object sender, EventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = textBox1.Text;
            if (fbd.ShowDialog() == DialogResult.OK) {
                textBox1.Text = fbd.SelectedPath;
                button1_Click(this, null);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect=DragDropEffects.Move;
            else
                e.Effect=DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0) textBox1.Text = files[0];
                button1_Click(this, null);
            }
        }
    }
}