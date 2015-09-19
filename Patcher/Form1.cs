using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Patcher
{
    public partial class Form1 : Form
    {

        RegistryKey regKey = Registry.CurrentUser;
        public Form1()
        {
            InitializeComponent();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = 0;
            regKey = regKey.OpenSubKey(@"Software\Valve\Steam");
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }


        public static void ReplaceTextInFile(string inFile, string find, string replace)
        {

            //if (find.Length != replace.Length) throw new ArgumentException("The lenght of find and replace strings must match!");

            const int chunkPrefix = 1024 * 10;
            //var findBytes = GetBytes(find);
            byte[] bytes = { 0x74, 0x63, 0x68, 0x5F, 0x6D, 0x61, 0x78, 0x00, 0x00, 0x00 };
            //MessageBox.Show(System.Text.Encoding.UTF8.GetString(bytes));
            var findBytes = bytes;
            var replaceBytes = Combine(bytes, GetBytes(replace));
            long chunkSize = findBytes.Length * chunkPrefix;
            var f = new FileInfo(inFile);
            //if (f.Length < chunkSize)
            //    chunkSize = f.Length;

            var readBuffer = new byte[chunkSize];

            using (Stream stream = File.Open(inFile, FileMode.Open))
            {
                int bytesRead;
                bool founded = false;
                while ((bytesRead = stream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                {
                    var replacePositions = new List<int>();
                    var matches = SearchBytePattern(findBytes, readBuffer, ref replacePositions);
                    if (matches != 0)
                        founded = true;
                        foreach (var replacePosition in replacePositions)
                        {
                            var originalPosition = stream.Position;
                            stream.Position = originalPosition - bytesRead + replacePosition;
                            stream.Write(replaceBytes, 0, replaceBytes.Length);
                            stream.Position = originalPosition;
                        }


                    if (stream.Length == stream.Position) break;
                    var moveBackByHalf = stream.Position - (bytesRead / 2);
                    stream.Position = moveBackByHalf;
                }

                if (founded == false)
                {
                    MessageBox.Show("Sequence not found! Wrong file?");
                }
                else
                {
                    MessageBox.Show("Successful patched!");
                }

            }

        }


        static public int SearchBytePattern(byte[] pattern, byte[] bytes, ref List<int> position)
        {
            int matches = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch == true; j++)
                    {
                        if (bytes[i + j] != pattern[j])
                            ismatch = false;
                    }
                    if (ismatch)
                    {
                       // MessageBox.Show("Patched!");
                        position.Add(i);
                        matches++;
                        i += pattern.Length - 1;
                    }
                }
            }

            return matches;
        }

        public static byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (regKey != null)
            {
                string bit = "win64";
                if (comboBox1.SelectedIndex == 1)
                {
                    bit = "win32";
                }
                string installpath = regKey.GetValue("SteamPath").ToString().Replace("/", @"\") + @"\steamapps\common\dota 2 beta\game\dota\bin\" + bit + @"\client.dll";
                if (File.Exists(installpath))
                {
                    ReplaceTextInFile(@installpath, "1134", textBox1.Text);
                }
                else
                {
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string source = openFileDialog1.FileName;
                        ReplaceTextInFile(@source, "1134", textBox1.Text);
                    }
                }

            }
            else
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string source = openFileDialog1.FileName;
                    ReplaceTextInFile(@source, "1134", textBox1.Text);
                }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
    }
}
